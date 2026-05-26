using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Utils;

public enum Weather
{
    Sunny, Rain, Hail, Sandstorm, None
}

public enum Outcome
{
    Win, Loss, Escaped, Caught, Undecided
}

public class EffectCommand
{
    public Effect Effect { get; set; }
    public Pokemon User { get; set; }
    public Pokemon Target { get; set; }
    public EffectCommand(Effect effect, Pokemon user, Pokemon target) { Effect = effect; User = user; Target = target; }
}

public class MoveCommand
{
    public Move Move { get; set; }
    public Pokemon User { get; set; }
    public Pokemon Target { get; set; }
    public MoveCommand(Move move, Pokemon user) { Move = move; User = user; }
    public MoveCommand(Move move, Pokemon user, Pokemon target) { Move = move; User = user; Target = target; }
}

public class SwitchCommand
{
    public Pokemon SwitchedIn { get; set; }
    public Pokemon SwitchedOut { get; set; }
    public SwitchCommand(Pokemon switchedIn, Pokemon switchedOut) { SwitchedIn = switchedIn; SwitchedOut = switchedOut; }
}

public class ItemCommand
{
    public Item Item { get; set; }
    public Pokemon Target { get; set; }
    public ItemCommand(Item item, Pokemon target) { Item = item; Target = target; }
}

public class BattleLogic
{
    private IBattle battleUI;
    private List<EffectCommand> effectQueue;
    private int orderIndex;
    private List<Pokemon> order;

    // 배틀이 이미 끝났는지 확인해서 중복 처리 방지
    private bool battleEnded;

    public int BattleSize { get; set; }
    public int TurnNumber { get; set; }
    public bool IsTrainerBattle { get; set; }
    public List<Pokemon> PartyAllies { get; set; }
    public List<Pokemon> ActiveAllies { get; set; }
    public List<Pokemon> PartyEnemies { get; set; }
    public List<Pokemon> ActiveEnemies { get; set; }
    public Weather Weather { get; set; }
    public Outcome Outcome { get; set; }
    public bool? Confirmation { get; set; }
    public int? MoveLearningSelection { get; set; }
    public BattleAnimations Animations { get; set; }

    public BattleLogic(IBattle battle, BattleInfo info, BattleAnimations anims)
    {
        battleUI = battle;
        Animations = anims;
        BattleSize = info.BattleSize;
        ActiveAllies = info.Allies.GetRange(0, info.BattleSize);
        PartyAllies = info.Allies.GetRange(info.BattleSize, info.Allies.Count - info.BattleSize);
        ActiveEnemies = info.Enemies.GetRange(0, info.BattleSize);
        PartyEnemies = info.Enemies.GetRange(info.BattleSize, info.Enemies.Count - info.BattleSize);
        Weather = info.Weather;
        Outcome = Outcome.Undecided;
        TurnNumber = 1;
        IsTrainerBattle = info.IsTrainerBattle;
        effectQueue = new List<EffectCommand>();
        battleEnded = false;

        info.Allies.ForEach(pkmn => pkmn.IsAlly = true);
        info.Enemies.ForEach(pkmn => pkmn.IsAlly = false);

        foreach (var pkmn in ActivePokemons())
        {
            if (!pkmn.IsAlly) pkmn.ExpCandidates = new List<Pokemon>(ActiveAllies);
        }
    }

    public List<Pokemon> SortBySpeed()
    {
        var actives = ActivePokemons();
        actives.Sort();
        return actives;
    }

    public List<Pokemon> ActivePokemons()
    {
        return ActiveAllies.Concat(ActiveEnemies).ToList();
    }

    public IEnumerator AddEffect(EffectLogic logic, Pokemon user, Pokemon target)
    {
        if (battleEnded) yield break;
        if (target.Health <= 0) yield break;

        var addedEffect = new Effect(logic);
        var cmd = new EffectCommand(addedEffect, user, target);
        if (EffectExists(addedEffect, user, target)) yield break;

        yield return battleUI.NotifyUpdateHealth();
        yield return addedEffect.Functions.OnCreation(addedEffect, user, target, battleUI);
        yield return battleUI.NotifyUpdateHealth();
        effectQueue.Add(cmd);
    }

    public bool EffectExists(Effect effect, Pokemon user, Pokemon target)
    {
        return effectQueue.Any(
            cmd => cmd != null &&
            cmd.Effect.Logic == effect.Logic &&
            (user == null || cmd.User == user) &&
            (target == null || cmd.Target == target)
        );
    }

    public bool EffectExists(EffectLogic logic, Pokemon user, Pokemon target)
    {
        return EffectExists(new Effect(logic), user, target);
    }

    public bool EffectExistsOnUser(Effect effect, Pokemon user)
    {
        return EffectExists(effect, user, null);
    }

    public bool EffectExistsOnUser(EffectLogic logic, Pokemon user)
    {
        return EffectExists(new Effect(logic), user, null);
    }

    public bool EffectExistsOnTarget(Effect effect, Pokemon target)
    {
        return EffectExists(effect, null, target);
    }

    public bool EffectExistsOnTarget(EffectLogic logic, Pokemon target)
    {
        return EffectExists(new Effect(logic), null, target);
    }

    public void TryEscape()
    {
        SceneInfo.SetForcedOutcome(Outcome.Escaped);
    }

    public IEnumerator SwitchPokemonImmediate(Pokemon switchedOut, Pokemon switchedIn)
    {
        if (battleEnded) yield break;

        var activeList = switchedIn.IsAlly ? ActiveAllies : ActiveEnemies;
        var partyList = switchedIn.IsAlly ? PartyAllies : PartyEnemies;

        activeList[activeList.FindIndex(pkmn => pkmn == switchedOut)] = switchedIn;
        partyList[partyList.FindIndex(pkmn => pkmn == switchedIn)] = switchedOut;

        yield return battleUI.RegisterSwitch(switchedIn);
        switchedIn.WasForcedSwitch = true;

        if (switchedIn.IsAlly)
        {
            foreach (var enemy in ActiveEnemies)
            {
                if (!enemy.ExpCandidates.Contains(switchedIn))
                    enemy.ExpCandidates.Add(switchedIn);
            }
        }
        else
        {
            switchedIn.ExpCandidates = new List<Pokemon>(ActiveAllies);
            switchedOut.ExpCandidates = null;
        }
    }

    public IEnumerator Init()
    {
        order = RecalculateOrder();

        foreach (var user in order)
        {
            if (battleEnded) yield break;

            yield return user.Ability.Functions.OnSwitchIn(user.Ability, user, battleUI);
            yield return battleUI.NotifyUpdateHealth();

            var outcome = CheckVictory();
            if (outcome != Outcome.Undecided)
            {
                yield return HandleBattleEndIfNeeded(outcome);
                yield break;
            }
        }

        battleUI.NotifyTurnFinished();
    }

    public IEnumerator Turn(Dictionary<Pokemon, MoveCommand> pendingMoves,
                            Dictionary<Pokemon, SwitchCommand> pendingSwitches,
                            Dictionary<Pokemon, bool> usedItems)
    {
        if (battleEnded) yield break;

        foreach (var user in ActivePokemons())
            user.HasActedThisTurn = false;

        if (Weather != Weather.None) yield return Print(WeatherToString());

        var outcome = CheckVictory();
        if (outcome != Outcome.Undecided)
        {
            yield return HandleBattleEndIfNeeded(outcome);
            battleUI.NotifyTurnFinished();
            yield break;
        }

        order = RecalculateOrder();

        foreach (var user in order)
            user.HasActedThisTurn = GetValue(usedItems, user);

        for (var i = 0; i < order.Count; i++)
        {
            if (battleEnded) yield break;

            var user = order[i];
            if (!user.WasForcedSwitch) continue;

            yield return user.Ability.Functions.OnSwitchIn(user.Ability, user, battleUI);
            yield return battleUI.NotifyUpdateHealth();

            for (var e = 0; e < effectQueue.Count; e++)
            {
                if (battleEnded) yield break;

                var cmd = effectQueue[e];
                if (cmd != null && cmd.Effect.Trigger == Trigger.OnSwitchIn)
                    yield return ApplyEffect(cmd, e, user);
            }

            user.HasActedThisTurn = false;
            user.WasForcedSwitch = false;
        }

        order = RecalculateOrder();

        for (var i = 0; i < order.Count; i++)
        {
            if (battleEnded) yield break;

            var user = order[i];
            var cmd = GetValue(pendingSwitches, user);

            if (cmd != null)
            {
                cmd.SwitchedIn.HasActedThisTurn = true;
                yield return SwitchPokemon(cmd, order);

                if (battleEnded) yield break;
            }
        }

        outcome = CheckVictory();
        if (outcome != Outcome.Undecided)
        {
            yield return HandleBattleEndIfNeeded(outcome);
            battleUI.NotifyTurnFinished();
            yield break;
        }

        order = RecalculateOrder();

        foreach (var user in order)
        {
            if (battleEnded) yield break;

            yield return user.Ability.Functions.OnTurnBeginning(user.Ability, user, battleUI);
            yield return battleUI.NotifyUpdateHealth();

            outcome = CheckVictory();
            if (outcome != Outcome.Undecided)
            {
                yield return HandleBattleEndIfNeeded(outcome);
                battleUI.NotifyTurnFinished();
                yield break;
            }
        }

        for (var i = 0; i < effectQueue.Count; i++)
        {
            if (battleEnded) yield break;

            var cmd = effectQueue[i];
            if (cmd != null && cmd.Effect.Trigger == Trigger.StartOfTurn)
                yield return ApplyEffect(cmd, i);
        }

        order = RecalculateOrder();

        while (!EveryoneHasActed())
        {
            if (battleEnded) yield break;

            if (orderIndex >= order.Count)
                break;

            var user = order[orderIndex];

            if (user.Health <= 0 || user.HasActedThisTurn)
            {
                orderIndex++;
                continue;
            }

            outcome = CheckVictory();
            if (outcome != Outcome.Undecided)
            {
                yield return HandleBattleEndIfNeeded(outcome);
                battleUI.NotifyTurnFinished();
                yield break;
            }

            var cmd = GetValue(pendingMoves, user);

            if (user.CanAttack && cmd != null)
            {
                user.HasActedThisTurn = true;

                var move = cmd.Move;
                if (move.MaxPoints > 0) move.Points--;

                yield return Print($"{user.Name}은(는) {move.Name}을(를) 사용했다!");
                yield return user.Ability.Functions.OnMoveUse(user.Ability, user, move, battleUI);

                var targetList = GetMoveTargets(cmd);

                if (targetList.Count == 0)
                {
                    yield return Print("하지만 실패했다!");
                }
                else
                {
                    foreach (var target in targetList)
                    {
                        if (battleEnded) yield break;

                        yield return move.Functions.OnUse(move, user, target, battleUI, targetList.Count);
                        yield return battleUI.NotifyUpdateHealth();

                        if (IsHit(move, user, target))
                        {
                            if (target.Health > 0)
                            {
                                LastMoveWasCrit = false;
                                PlayEffectivenessSound(move, target);

                                yield return move.Functions.Execute(move, user, target, battleUI, targetList.Count);
                                yield return battleUI.NotifyUpdateHealth();

                                target.LastHitByMove = move;
                                target.LastHitByUser = user;

                                if (LastMoveWasCrit) yield return Print("급소에 맞았다!");

                                yield return PrintEffectiveness(move, target);
                                yield return move.Functions.OnHit(move, user, target, battleUI, targetList.Count);
                            }
                        }
                        else
                        {
                            yield return Print("하지만 빗나갔다!");
                            yield return move.Functions.OnMiss(move, user, target, battleUI, targetList.Count);
                            yield return battleUI.NotifyUpdateHealth();
                        }
                    }
                }

                yield return user.Ability.Functions.AfterMoveUse(user.Ability, user, move, battleUI);
                yield return CheckDeath();
                yield return battleUI.NotifyUpdateHealth();

                if (battleEnded) yield break;

                outcome = CheckVictory();
                if (outcome != Outcome.Undecided)
                {
                    yield return HandleBattleEndIfNeeded(outcome);
                    battleUI.NotifyTurnFinished();
                    yield break;
                }
            }

            order = RecalculateOrder();
        }

        for (var i = 0; i < effectQueue.Count; i++)
        {
            if (battleEnded) yield break;

            var cmd = effectQueue[i];
            if (cmd == null) continue;

            var effect = effectQueue[i].Effect;

            if (effect.Trigger == Trigger.EndOfTurn)
                yield return ApplyEffect(cmd, i);

            if (battleEnded) yield break;

            if (cmd.Target.IsAlly ? ActiveAllies.Contains(cmd.Target) : ActiveEnemies.Contains(cmd.Target))
                effect.Turn++;
        }

        order = RecalculateOrder();

        foreach (var user in order)
        {
            if (battleEnded) yield break;

            if (user.Health > 0)
            {
                yield return user.Ability.Functions.OnTurnEnding(user.Ability, user, battleUI);
                yield return battleUI.NotifyUpdateHealth();

                outcome = CheckVictory();
                if (outcome != Outcome.Undecided)
                {
                    yield return HandleBattleEndIfNeeded(outcome);
                    battleUI.NotifyTurnFinished();
                    yield break;
                }

                user.Ability.Turn++;
            }
        }

        TurnNumber++;

        outcome = CheckVictory();
        if (outcome != Outcome.Undecided)
        {
            yield return HandleBattleEndIfNeeded(outcome);
            battleUI.NotifyTurnFinished();
            yield break;
        }

        battleUI.NotifyTurnFinished();
    }

    private IEnumerator Print(string message, bool delay = true)
    {
        yield return battleUI.Print(message, delay);
    }

    // 람브전인지 확인
    private bool IsLambeBattle()
    {
        return ActiveEnemies.Concat(PartyEnemies).Any(enemy =>
            enemy != null &&
            (
                enemy.Name.Contains("람브") ||
                enemy.Skeleton.pokemonName.Contains("람브")
            )
        );
    }

    // 람브전에서 아군이 쓰러졌을 때 교체창 없이 맵으로 복귀
    private IEnumerator ReturnToMapAfterLambeDefeat()
    {
        if (battleEnded)
            yield break;

        battleEnded = true;
        Outcome = Outcome.Loss;

        yield return Print("람브의 힘은 아직 감당하기 어렵다...");
        yield return Print("레벨 100이 된 뒤 다시 도전하자!");
        yield return new WaitForSeconds(1f);

        SceneInfo.ReturnToOverworld();
    }

    private IEnumerator HandleBattleEndIfNeeded(Outcome result)
    {
        if (battleEnded)
            yield break;

        if (result == Outcome.Undecided)
            yield break;

        battleEnded = true;
        Outcome = result;

        if (result == Outcome.Loss)
        {
            yield return Print("눈앞이 깜깜해졌다...");
            yield return new WaitForSeconds(1f);
            SceneInfo.ReturnToOverworld();
            yield break;
        }

        if (result == Outcome.Win)
        {
            yield return Print("승리했다!");
            yield return new WaitForSeconds(1f);
            SceneInfo.ReturnToOverworld();
            yield break;
        }

        if (result == Outcome.Escaped || result == Outcome.Caught)
        {
            yield return new WaitForSeconds(0.3f);
            SceneInfo.ReturnToOverworld();
            yield break;
        }
    }

    private bool EveryoneHasActed()
    {
        return order.All(pokemon => pokemon.HasActedThisTurn || pokemon.Health <= 0);
    }

    private List<Pokemon> RecalculateOrder()
    {
        orderIndex = 0;
        return SortBySpeed();
    }

    private void PlayEffectivenessSound(Move move, Pokemon target)
    {
        if (move.Category == MoveCategory.Status) return;

        var multiplier = Types.Affinity(move, target);

        if (multiplier == 0f) return;
        else if (multiplier < 1f) battleUI.PlayNotVeryEffectiveHitSound();
        else if (multiplier >= 2f) battleUI.PlaySuperEffectiveHitSound();
        else battleUI.PlayHitSound();
    }

    private IEnumerator PrintEffectiveness(Move move, Pokemon target)
    {
        if (move.Category == MoveCategory.Status) yield break;

        var multiplier = Types.Affinity(move, target);

        if (multiplier == 0f) yield return Print("효과가 없는 것 같다...");
        else if (multiplier < 1f) yield return Print("효과가 별로인 듯하다...");
        else if (multiplier >= 2f) yield return Print("효과가 굉장했다!");
    }

    private IEnumerator SwitchPokemon(SwitchCommand cmd, List<Pokemon> order)
    {
        if (battleEnded) yield break;

        cmd.SwitchedOut.Ability.Functions.OnSwitchOut(cmd.SwitchedOut.Ability, cmd.SwitchedOut, battleUI);

        for (var i = 0; i < effectQueue.Count; i++)
        {
            if (battleEnded) yield break;

            var effectCommand = effectQueue[i];
            if (effectCommand == null) continue;

            if (effectCommand.Target == cmd.SwitchedOut)
                effectCommand.Effect.Functions.OnSwitchOut(effectCommand.Effect, effectCommand.User, effectCommand.Target, battleUI);

            if (effectCommand.Effect.Trigger == Trigger.OnSwitchOut && effectCommand.Target == cmd.SwitchedOut)
            {
                yield return ApplyEffect(effectCommand, i);

                if (effectCommand.Effect.EndOnSwitch)
                    effectQueue[i] = null;
            }
        }

        yield return CheckDeath();

        if (battleEnded) yield break;

        var outcome = CheckVictory();
        if (outcome != Outcome.Undecided)
        {
            yield return HandleBattleEndIfNeeded(outcome);
            yield break;
        }

        var activeList = cmd.SwitchedIn.IsAlly ? ActiveAllies : ActiveEnemies;
        var partyList = cmd.SwitchedIn.IsAlly ? PartyAllies : PartyEnemies;

        activeList[activeList.FindIndex(pkmn => pkmn == cmd.SwitchedOut)] = cmd.SwitchedIn;
        partyList[partyList.FindIndex(pkmn => pkmn == cmd.SwitchedIn)] = cmd.SwitchedOut;
        order[order.FindIndex(pkmn => pkmn == cmd.SwitchedOut)] = cmd.SwitchedIn;

        yield return battleUI.RegisterSwitch(cmd.SwitchedIn);

        if (cmd.SwitchedIn.IsAlly)
        {
            foreach (var enemy in ActiveEnemies)
            {
                if (!enemy.ExpCandidates.Contains(cmd.SwitchedIn))
                    enemy.ExpCandidates.Add(cmd.SwitchedIn);
            }
        }
        else
        {
            cmd.SwitchedIn.ExpCandidates = new List<Pokemon>(ActiveAllies);
            cmd.SwitchedOut.ExpCandidates = null;
        }

        cmd.SwitchedIn.Ability.Functions.OnSwitchIn(cmd.SwitchedIn.Ability, cmd.SwitchedIn, battleUI);

        for (var i = 0; i < effectQueue.Count; i++)
        {
            if (battleEnded) yield break;

            var effectCommand = effectQueue[i];

            if (effectCommand != null && effectCommand.Effect.Trigger == Trigger.OnSwitchIn)
                yield return ApplyEffect(effectCommand, i, cmd.SwitchedIn);
        }

        yield return CheckDeath();

        if (battleEnded) yield break;

        battleUI.UpdateMoveTargets(cmd);
    }

    private List<Pokemon> GetMoveTargets(MoveCommand cmd)
    {
        if (cmd == null) return new List<Pokemon>();

        List<Pokemon> targets = null;
        var targeting = cmd.Move.Targeting;

        switch (targeting)
        {
            case Targeting.Self:
                targets = new List<Pokemon>() { cmd.User };
                break;

            case Targeting.Single:
                targets = IsActive(cmd.Target) ? new List<Pokemon>() { cmd.Target } : new List<Pokemon>();
                break;

            case Targeting.Adjacent:
                var primaryTarget = cmd.Target;
                var targetList = primaryTarget.IsAlly ? ActiveAllies : ActiveEnemies;
                var index = targetList.FindIndex(pkmn => pkmn == primaryTarget);

                targets = new List<Pokemon>();

                if (index - 1 >= 0) targets.Add(targetList[index - 1]);
                targets.Add(targetList[index]);
                if (index + 1 < targetList.Count) targets.Add(targetList[index + 1]);
                break;

            case Targeting.Allies:
                targets = cmd.User.IsAlly ? ActiveAllies : ActiveEnemies;
                break;

            case Targeting.Enemies:
                targets = cmd.User.IsAlly ? ActiveEnemies : ActiveAllies;
                break;

            case Targeting.All:
                targets = ActivePokemons();
                break;
        }

        return targeting == Targeting.Self ? targets : targets.FindAll(pkmn => pkmn.Health > 0);
    }

    private IEnumerator ApplyEffect(EffectCommand cmd, int index, Pokemon specificTarget)
    {
        if (battleEnded) yield break;
        if (cmd == null) yield break;

        var target = specificTarget ?? cmd.Target;

        if (target.IsAlly ? !ActiveAllies.Contains(target) : !ActiveEnemies.Contains(target))
            yield break;

        if (cmd.Effect.Turn > cmd.Effect.Duration)
        {
            if (target.Health > 0)
                yield return cmd.Effect.Functions.OnDeletion(cmd.Effect, cmd.User, cmd.Target, battleUI);

            effectQueue[index] = null;
            yield return battleUI.NotifyUpdateHealth();
        }
        else
        {
            if (target.Health > 0)
                yield return cmd.Effect.Functions.Execute(cmd.Effect, cmd.User, cmd.Target, battleUI);

            yield return battleUI.NotifyUpdateHealth();
        }

        yield return CheckDeath();
    }

    private IEnumerator ApplyEffect(EffectCommand cmd, int index)
    {
        yield return ApplyEffect(cmd, index, null);
    }

    private IEnumerator CheckDeath()
    {
        foreach (var user in order)
        {
            if (battleEnded) yield break;

            if (user.Health <= 0 && user.Status != Status.Fainted)
            {
                yield return battleUI.NotifyUpdateHealth();

                Animations.GetUnit(user).PlayFaintCry();
                yield return Print($"{user.Name}은(는) 쓰러졌다!");
                yield return Animations.Faint(user);

                user.Status = Status.Fainted;

                // ★ 핵심 추가:
                // 람브전에서는 아군 포켓몬이 하나라도 쓰러지면
                // "내보낼 포켓몬을 선택하자!"로 가지 않고 멘트 후 맵으로 복귀
                if (user.IsAlly && IsLambeBattle())
                {
                    yield return ReturnToMapAfterLambeDefeat();
                    yield break;
                }

                for (var i = 0; i < effectQueue.Count; i++)
                {
                    if (battleEnded) yield break;

                    var cmd = effectQueue[i];

                    if (cmd != null && cmd.User == user && cmd.Effect.Trigger == Trigger.OnDeath)
                        yield return ApplyEffect(cmd, i);
                }

                yield return user.Ability.Functions.OnDeath(user.Ability, user, battleUI);
                yield return battleUI.NotifyUpdateHealth();

                var outcome = CheckVictory();

                if (outcome == Outcome.Loss)
                {
                    yield return HandleBattleEndIfNeeded(outcome);
                    yield break;
                }

                if (!user.IsAlly)
                {
                    var expList = user.ExpCandidates.FindAll(pkmn => pkmn.Health > 0);

                    foreach (var candidate in expList)
                    {
                        if (battleEnded) yield break;

                        var expReward = GetExpForKill(candidate, user, expList.Count, IsTrainerBattle);
                        yield return HandleExpGain(candidate, expReward);
                    }
                }

                order = RecalculateOrder();

                outcome = CheckVictory();
                if (outcome != Outcome.Undecided)
                {
                    yield return HandleBattleEndIfNeeded(outcome);
                    yield break;
                }
            }
        }
    }

    private IEnumerator HandleExpGain(Pokemon receiver, int expGained)
    {
        if (battleEnded) yield break;

        receiver.Experience += expGained;
        yield return Print($"{receiver.Name}은(는) 경험치를 {expGained} 얻었다!");

        var hasPendingEvolution = false;
        var targetLevel = GetLevelFromExp(receiver.Experience, receiver.ExpGroup);

        while (targetLevel > receiver.Level)
        {
            if (battleEnded) yield break;

            receiver.LevelUp();

            if (ActiveAllies.Contains(receiver))
            {
                yield return battleUI.NotifyUpdateExp(true);
                yield return battleUI.NotifyUpdateHealth(true);
            }

            battleUI.PlayLevelUpSound();

            yield return Print($"{receiver.Name}은(는) 레벨 {receiver.Level}이 되었다!");
            yield return HandleMoveLearning(receiver);

            if (receiver.Skeleton.levelEvolution != null &&
                receiver.Level >= receiver.Skeleton.levelEvolution.level &&
                !hasPendingEvolution)
            {
                hasPendingEvolution = true;
                SceneInfo.AddPendingEvolution(receiver, receiver.Skeleton.levelEvolution.evolution);
            }
        }

        if (ActiveAllies.Contains(receiver))
            yield return battleUI.NotifyUpdateExp(false);
    }

    private IEnumerator HandleMoveLearning(Pokemon learner)
    {
        foreach (var moveSkeleton in learner.NewMovesFromLevelUp())
        {
            if (battleEnded) yield break;

            Confirmation = null;
            MoveLearningSelection = null;

            var usedSlots = learner.GetFilledMoveSlots();

            if (usedSlots < 4)
            {
                learner.Moves[usedSlots] = new Move(moveSkeleton);
                yield return Print($"{learner.Name}은(는) {moveSkeleton.moveName}을(를) 배웠다!");
                continue;
            }

            yield return Print($"{learner.Name}은(는) {moveSkeleton.moveName}을(를) 배우려 한다.");
            yield return Print("배우게 할까?", false);

            battleUI.RequestConfirmationBox();
            yield return Await(() => Confirmation != null);

            if (Confirmation == false)
                continue;

            yield return Print("어떤 기술을 잊게 할까?");

            battleUI.RequestMoveReplacement(learner);
            yield return Await(() => MoveLearningSelection != null);
            battleUI.GoIdle();

            if (MoveLearningSelection < 0)
            {
                yield return Print($"{learner.Name}은(는) {moveSkeleton.moveName}을(를) 배우지 않았다.");
                continue;
            }

            learner.Moves[MoveLearningSelection.Value] = new Move(moveSkeleton);
            yield return Print($"{learner.Name}은(는) {moveSkeleton.moveName}을(를) 배웠다!");

            if (ActiveAllies.Contains(learner))
                battleUI.RefreshMoves(learner);
        }
    }

    private string WeatherToString()
    {
        switch (Weather)
        {
            case Weather.Hail:
                return "싸라기눈이 내리고 있다.";

            case Weather.Rain:
                return "비가 내리고 있다.";

            case Weather.Sandstorm:
                return "모래바람이 몰아치고 있다.";

            case Weather.Sunny:
                return "햇살이 강하다.";

            default:
                return null;
        }
    }

    private bool IsActive(Pokemon pokemon)
    {
        return pokemon.Health > 0 && ActivePokemons().Contains(pokemon);
    }

    private Outcome CheckVictory()
    {
        var forcedOutcome = SceneInfo.ConsumeForcedOutcome();

        if (forcedOutcome != Outcome.Undecided)
        {
            Outcome = forcedOutcome;
            return Outcome;
        }

        if (PartyAllies.Concat(ActiveAllies).All(pkmn => pkmn.Health <= 0))
            Outcome = Outcome.Loss;
        else if (PartyEnemies.Concat(ActiveEnemies).All(pkmn => pkmn.Health <= 0))
            Outcome = Outcome.Win;
        else
            Outcome = Outcome.Undecided;

        return Outcome;
    }
}