using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using static Utils;

/// <summary>
/// UI and logic manager for a 1v1 battle.
/// </summary>
public class SingleBattle : MonoBehaviour, IBattle
{
    public GameObject battleCanvas;
    public GameObject partyCanvas;
    public GameObject bagCanvas;
    public Unit playerUnit;
    public Unit enemyUnit;
    public HUD hud;
    public BattleAnimations anims;
    public Dialog chatbox;
    public BattleParty party;
    public BattleBag bag;
    public AudioSource audioPlayer;
    public AudioSource chatSound;
    public AudioSource hitSound;
    public AudioSource notVeryEffectiveSound;
    public AudioSource superEffectiveSound;
    public AudioSource levelUpSound;
    public BattleBackgroundManager backgroundManager;

    private int orderIndex;
    private int actionIndex;
    private int moveIndex;
    private int confirmationIndex;
    private int forcedSwitchIndex;
    private bool isForcedSwitch;

    private List<Pokemon> order;
    private Dictionary<Pokemon, SwitchCommand> pendingSwitches;
    private Dictionary<Pokemon, MoveCommand> pendingMoves;
    private Dictionary<Pokemon, bool> usedItems;

    public BattleState BattleState { get; set; }
    public BattleLogic Logic { get; private set; }
    public IDialog Chatbox { get { return chatbox; } }
    public BattleInfo BattleInfo { get; private set; }
    public PlayerInfo PlayerInfo { get; private set; }

    public IEnumerator Print(string message, bool delay = true)
    {
        yield return chatbox.Print(message);
        if (delay) yield return new WaitForSeconds(1f);
    }

    void Start()
    {
        BattleInfo = SceneInfo.GetBattleInfo();

        if (backgroundManager != null)
            backgroundManager.Apply();

        PlayerInfo = SceneInfo.GetPlayerInfo();

        EnsureAllLeadingPokemonAlive(BattleInfo);

        Logic = new BattleLogic(this, BattleInfo, anims);
        pendingMoves = new Dictionary<Pokemon, MoveCommand>();
        pendingSwitches = new Dictionary<Pokemon, SwitchCommand>();
        usedItems = new Dictionary<Pokemon, bool>();
        order = Logic.SortBySpeed();

        Application.targetFrameRate = 60;

        playerUnit.Setup(Logic.ActiveAllies[0], true);
        enemyUnit.Setup(Logic.ActiveEnemies[0], BattleInfo.IsTrainerBattle);
        hud.Init(playerUnit.Pokemon, enemyUnit.Pokemon);

        chatbox.RefreshMoves(playerUnit.Pokemon);

        if (chatbox.confirmationObject != null)
            chatbox.confirmationObject.SetActive(false);

        if (BattleInfo.IsTrainerBattle) anims.SetupNPCIntro(BattleInfo.Trainer);
        else anims.DisableNPCIntro();

        BattleState = BattleState.Intro;
        StartCoroutine(BattleIntro());
    }

    private bool PressedEnter()
    {
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    private bool PressedEsc()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }

    private void PlayChatSound()
    {
        if (chatSound != null)
            chatSound.Play();
    }

    void Update()
    {
        switch (BattleState)
        {
            case BattleState.SelectingAction:
                ActionPicker();
                break;

            case BattleState.SelectingMove:
            case BattleState.SelectingReplacedMove:
                MovePicker();
                break;

            case BattleState.Confirming:
                ConfirmationPicker();
                break;
        }
    }

    // =========================
    // 액션 버튼 색상 처리
    // =========================

    private void ResetActionTextColors()
    {
        for (int i = 0; i < chatbox.actions.Length; i++)
        {
            chatbox.actions[i].color = Color.black;
        }
    }

    private void SetActionColor(int index, Color color)
    {
        ResetActionTextColors();

        if (index >= 0 && index < chatbox.actions.Length)
        {
            chatbox.actions[index].color = color;
        }
    }

    public void OnHoverAction0() { SetActionColor(0, Color.blue); }
    public void OnHoverAction1() { SetActionColor(1, Color.blue); }
    public void OnHoverAction2() { SetActionColor(2, Color.blue); }
    public void OnHoverAction3() { SetActionColor(3, Color.blue); }

    public void OnExitAction()
    {
        ResetActionTextColors();
    }

    // =========================
    // 스킬 버튼 색상 처리
    // =========================

    private void ResetMoveTextColors()
    {
        for (int i = 0; i < chatbox.moves.Length; i++)
        {
            chatbox.moves[i].color = Color.black;
        }
    }

    private void SetMoveColor(int index, Color color)
    {
        ResetMoveTextColors();

        if (index >= 0 && index < chatbox.moves.Length)
        {
            if (chatbox.moves[index].text != "-")
                chatbox.moves[index].color = color;
        }
    }

    public void OnHoverMove0() { SetMoveColor(0, Color.blue); }
    public void OnHoverMove1() { SetMoveColor(1, Color.blue); }
    public void OnHoverMove2() { SetMoveColor(2, Color.blue); }
    public void OnHoverMove3() { SetMoveColor(3, Color.blue); }

    public void OnExitMove()
    {
        ResetMoveTextColors();
    }

    // =========================
    // 액션 마우스 클릭
    // =========================

    public void OnClickFight()
    {
        if (BattleState != BattleState.SelectingAction) return;
        if (chatbox.IsBusy) return;

        actionIndex = 0;
        SetActionColor(0, Color.blue);
        PlayChatSound();
        BeginPlayerMove();
    }

    public void OnClickBag()
    {
        if (BattleState != BattleState.SelectingAction) return;
        if (chatbox.IsBusy) return;

        actionIndex = 1;
        SetActionColor(1, Color.blue);
        PlayChatSound();
        StartCoroutine(BeginOpenBag());
    }

    public void OnClickParty()
    {
        if (BattleState != BattleState.SelectingAction) return;
        if (chatbox.IsBusy) return;

        actionIndex = 2;
        SetActionColor(2, Color.blue);
        PlayChatSound();
        isForcedSwitch = false;
        StartCoroutine(BeginSwitch());
    }

    public void OnClickRun()
    {
        if (BattleState != BattleState.SelectingAction) return;
        if (chatbox.IsBusy) return;

        actionIndex = 3;
        SetActionColor(3, Color.blue);
        PlayChatSound();
        Logic.TryEscape();
        AddItemCommand();
    }

    // =========================
    // 스킬 마우스 클릭
    // =========================

    public void OnClickMove0() { SelectMoveByMouse(0); }
    public void OnClickMove1() { SelectMoveByMouse(1); }
    public void OnClickMove2() { SelectMoveByMouse(2); }
    public void OnClickMove3() { SelectMoveByMouse(3); }

    public void OnClickBackFromMove()
    {
        BackFromMoveSelection();
    }

    private void SelectMoveByMouse(int index)
    {
        if (BattleState != BattleState.SelectingMove && BattleState != BattleState.SelectingReplacedMove) return;
        if (chatbox.IsBusy) return;
        if (index < 0 || index >= order[orderIndex].GetFilledMoveSlots()) return;
        if (chatbox.moves[index].text == "-") return;

        moveIndex = index;
        SetMoveColor(index, Color.blue);
        PlayChatSound();

        if (BattleState == BattleState.SelectingReplacedMove)
        {
            Logic.MoveLearningSelection = moveIndex;
            return;
        }

        AddMoveCommand(order[orderIndex].Moves[moveIndex], enemyUnit.Pokemon);
    }

    private void BackFromMoveSelection()
    {
        if (BattleState != BattleState.SelectingMove && BattleState != BattleState.SelectingReplacedMove) return;
        if (chatbox.IsBusy) return;

        ResetMoveTextColors();
        PlayChatSound();

        if (BattleState == BattleState.SelectingReplacedMove)
        {
            Logic.MoveLearningSelection = -1;
            return;
        }

        BeginPlayerAction(true);
    }

    private IEnumerator BattleIntro()
    {
        chatbox.SetState(ChatState.None);
        yield return hud.IntroEffect();

        if (BattleInfo.IsTrainerBattle) yield return TrainerBattleIntro();
        else yield return WildBattleIntro();

        yield return Logic.Init();
        BeginPlayerAction();
    }

    private IEnumerator WildBattleIntro()
    {
        chatbox.SetState(ChatState.ChatOnly);
        hud.ShowEnemyHUD();
        enemyUnit.PlayCry();
        yield return Print($"야생의 {enemyUnit.Name}이(가) 나타났다!");

        yield return Print($"가라! {playerUnit.Pokemon.Name}!");
        hud.ShowAllyHUD();
        playerUnit.PlayEnterCry();
        yield return anims.SwitchInPokemon(playerUnit.Pokemon);
    }

    private IEnumerator TrainerBattleIntro()
    {
        var trainer = BattleInfo.Trainer;

        yield return anims.PlayNPCIntro();
        chatbox.SetState(ChatState.ChatOnly);
        yield return Print($"{trainer.skeleton.className} {trainer.Name}이(가) 승부를 걸어왔다!");
        yield return anims.PlayNPCSlideOut();

        yield return Print($"{trainer.skeleton.className} {trainer.Name}은(는) {enemyUnit.Name}을(를) 내보냈다!");
        hud.ShowEnemyHUD();
        enemyUnit.PlayEnterCry();
        yield return anims.SwitchInPokemon(enemyUnit.Pokemon);

        yield return Print($"가라! {playerUnit.Pokemon.Name}!");
        hud.ShowAllyHUD();
        playerUnit.PlayEnterCry();
        yield return anims.SwitchInPokemon(playerUnit.Pokemon);
    }

    private void EnsureAllLeadingPokemonAlive(BattleInfo info)
    {
        for (var i = 0; i < info.BattleSize && i < info.Allies.Count; i++)
        {
            if (info.Allies[i].Status == Status.Fainted)
            {
                var swap = info.Allies[i];
                var alive = info.Allies.FindIndex(i + 1, pkmn => pkmn.Health > 0);
                info.Allies[i] = info.Allies[alive];
                info.Allies[alive] = swap;
            }
        }
    }

    private IEnumerator MoveToNextForcedSwitch()
    {
        var actives = Logic.ActivePokemons();

        while (forcedSwitchIndex < actives.Count && actives[forcedSwitchIndex].Health > 0)
            forcedSwitchIndex++;

        if (forcedSwitchIndex < actives.Count)
        {
            if (actives[forcedSwitchIndex].IsAlly)
            {
                isForcedSwitch = true;
                StartCoroutine(BeginSwitch());
            }
            else
            {
                var switchedIn = RandomElement(Logic.PartyEnemies.FindAll(pkmn => pkmn.Health > 0));
                yield return Logic.SwitchPokemonImmediate(Logic.ActivePokemons()[forcedSwitchIndex], switchedIn);
                yield return MoveToNextForcedSwitch();
            }
        }
        else
        {
            order = Logic.SortBySpeed();
            BeginPlayerAction();
        }
    }

    private IEnumerator PerformForcedSwitches()
    {
        BattleState = BattleState.Idle;
        yield return MoveToNextForcedSwitch();
    }

    public void NotifyTurnFinished()
    {
        switch (Logic.Outcome)
        {
            case Outcome.Undecided:
                orderIndex = 0;
                forcedSwitchIndex = 0;
                pendingMoves = new Dictionary<Pokemon, MoveCommand>();
                pendingSwitches = new Dictionary<Pokemon, SwitchCommand>();
                usedItems = new Dictionary<Pokemon, bool>();
                StartCoroutine(PerformForcedSwitches());
                break;

            case Outcome.Win:
                StartCoroutine(OnWin());
                break;

            case Outcome.Loss:
                StartCoroutine(OnLoss());
                break;

            case Outcome.Escaped:
                StartCoroutine(OnEscape());
                break;

            case Outcome.Caught:
                StartCoroutine(OnCaught());
                break;
        }
    }

    private IEnumerator OnWin()
    {
        if (BattleInfo.IsTrainerBattle)
        {
            var dialogue = BattleInfo.Trainer.defeatDialogue;
            var index = 1;

            BattleInfo.Trainer.IsDefeated = true;
            PlayerInfo.Player.Money += BattleInfo.Trainer.money;

            SceneInfo.StopBattleMusic();
            audioPlayer.clip = BattleInfo.Trainer.skeleton.victoryMusic;
            audioPlayer.volume = 0.4f;
            audioPlayer.Play();

            hud.HideEnemyHUD();
            yield return anims.PlayNPCSlideIn();
            yield return Print(dialogue[0], false);

            while (index < dialogue.Length + 2)
            {
                if (PressedEnter())
                {
                    PlayChatSound();

                    if (index < dialogue.Length)
                        yield return Print(dialogue[index], false);
                    else if (index == dialogue.Length)
                        yield return Print($"{PlayerInfo.Player.Name}은(는) 승리하여 {BattleInfo.Trainer.money}원을 얻었다!", false);
                    else
                    {
                        BattleState = BattleState.Idle;
                        StartCoroutine(hud.ReturnToOverworld());
                    }

                    index++;
                }
                else yield return null;
            }
        }
        else
        {
            BattleState = BattleState.Idle;
            StartCoroutine(hud.ReturnToOverworld());
        }
    }

    private IEnumerator OnLoss()
    {
        var isTrainerBattle = BattleInfo.IsTrainerBattle;
        var moneyLoss = PlayerInfo.Player.Money / 2;
        var index = 0;

        PlayerInfo.Player.Money -= moneyLoss;

        yield return Print($"{PlayerInfo.Player.Name}은(는) 더 이상 싸울 포켓몬이 없다!", false);

        while (index <= 3)
        {
            if (PressedEnter())
            {
                PlayChatSound();

                switch (index++)
                {
                    case 0:
                        if (isTrainerBattle)
                            yield return Print($"{PlayerInfo.Player.Name}은(는) {moneyLoss}원을 지불했다...", false);
                        else
                            yield return Print($"{PlayerInfo.Player.Name}은(는) 도망치다 {moneyLoss}원을 잃었다...", false);
                        break;

                    case 1:
                        yield return Print("...", false);
                        break;

                    case 2:
                        yield return Print($"{PlayerInfo.Player.Name}은(는) 눈앞이 깜깜해졌다!", false);
                        break;

                    case 3:
                        BattleState = BattleState.Idle;
                        StartCoroutine(hud.ReturnToOverworld());
                        break;
                }
            }
            else yield return null;
        }
    }

    private IEnumerator OnEscape()
    {
        yield return Print("무사히 도망쳤다!", false);

        while (true)
        {
            if (PressedEnter())
            {
                BattleState = BattleState.Idle;
                StartCoroutine(hud.ReturnToOverworld());
                break;
            }
            else yield return null;
        }
    }

    private IEnumerator OnCaught()
    {
        var pokemonsInParty = PlayerInfo.Player.Pokemons;
        var index = 0;

        pokemonsInParty.Add(enemyUnit.Pokemon);
        yield return Print($"{enemyUnit.Pokemon.Name}을(를) 잡았다!", false);

        if (pokemonsInParty.Count < 6) index++;

        while (index <= 2)
        {
            if (PressedEnter())
            {
                PlayChatSound();

                switch (index++)
                {
                    case 0:
                        yield return Print("<포켓몬은 PC로 전송됩니다. 준비 중>", false);
                        break;

                    case 1:
                        yield return Print("<도감 데이터 준비 중>", false);
                        break;

                    case 2:
                        BattleState = BattleState.Idle;
                        StartCoroutine(hud.ReturnToOverworld());
                        break;
                }
            }
            else yield return null;
        }
    }

    public IEnumerator PresentSwitch(Pokemon switchedIn)
    {
        var originalState = BattleState;
        BattleState = BattleState.Idle;

        if (switchedIn.IsAlly)
            yield return Print($"가라! {switchedIn.Name}!");
        else
        {
            yield return Print($"{BattleInfo.Trainer.skeleton.className} {BattleInfo.Trainer.Name}은(는) {switchedIn.Name}을(를) 내보냈다!");
            hud.NotifySwitch(switchedIn);
        }

        anims.GetUnit(switchedIn).PlayEnterCry();
        yield return anims.SwitchInPokemon(switchedIn);

        BattleState = originalState;
    }

    public IEnumerator NotifyUpdateHealth(bool immediate = false)
    {
        hud.UpdateStatuses();
        yield return hud.UpdateAllyHealth(immediate);
        yield return hud.UpdateEnemyHealth(immediate);
    }

    public IEnumerator NotifyUpdateExp(bool fill)
    {
        yield return fill ? hud.FillAllyExpBar() : hud.UpdateAllyExp();
    }

    public void SwitchUpdateUI(Pokemon switchedIn)
    {
        if (switchedIn.IsAlly)
            playerUnit.Setup(switchedIn, true);
        else
            enemyUnit.Setup(switchedIn, true);
    }

    public IEnumerator NotifySwitchPerformed(Pokemon selection)
    {
        if (selection == null)
        {
            BeginPlayerAction();
            yield break;
        }

        if (isForcedSwitch)
        {
            yield return Logic.SwitchPokemonImmediate(Logic.ActivePokemons()[forcedSwitchIndex], selection);
            yield return MoveToNextForcedSwitch();
            yield break;
        }

        AddSwitchCommand(selection);
    }

    public void NotifyItemUsed(Item item)
    {
        if (item == null)
            BeginPlayerAction();
        else if (item.Usage == ItemUsage.TargetsAlly)
            AddItemCommand();
        else
            StartCoroutine(UseItemOnEnemy(item));
    }

    private IEnumerator UseItemOnEnemy(Item item)
    {
        chatbox.SetState(ChatState.ChatOnly);

        if (bag.chatbox.confirmationObject != null)
            bag.chatbox.confirmationObject.SetActive(false);

        var target = Logic.ActiveEnemies[0];

        if (!bag.ItemToUse.Functions.CanBeUsed(bag.ItemToUse, target))
        {
            yield return chatbox.Print($"지금은 {target.Name}에게 이 아이템을 사용할 수 없다.");

            while (!PressedEnter())
                yield return null;

            bag.Init(this);
            yield break;
        }

        yield return hud.FadeInTransition();
        bagCanvas.SetActive(false);
        battleCanvas.SetActive(true);
        yield return hud.FadeOutTransition();

        PlayerInfo.Player.Bag.TakeItem(bag.ItemToUse, bag.ItemToUseIndex, 1);
        yield return bag.ItemToUse.Functions.Use(bag.ItemToUse, target, chatbox, anims);
        yield return NotifyUpdateHealth();
        yield return bag.ItemToUse.Functions.OnUse(bag.ItemToUse, target, chatbox, anims);

        bag.ItemToUse = null;
        AddItemCommand();
    }

    public void AddSwitchCommand(Pokemon switchedIn)
    {
        pendingSwitches[order[orderIndex]] = new SwitchCommand(switchedIn, order[orderIndex]);
        MoveToNextInOrder();
    }

    public void AddMoveCommand(Move move, Pokemon target)
    {
        pendingMoves[order[orderIndex]] = new MoveCommand(move, order[orderIndex], target);
        MoveToNextInOrder();
    }

    public void AddMoveCommand(Move move)
    {
        pendingMoves[order[orderIndex]] = new MoveCommand(move, order[orderIndex]);
        MoveToNextInOrder();
    }

    public void AddItemCommand()
    {
        usedItems[order[orderIndex]] = true;
        MoveToNextInOrder();
    }

    public void UpdateMoveTargets(SwitchCommand cmd)
    {
        foreach (var key in pendingMoves.Keys)
        {
            if (pendingMoves[key].Target == cmd.SwitchedOut)
                pendingMoves[key].Target = cmd.SwitchedIn;
        }
    }

    public IEnumerator RegisterSwitch(Pokemon switchedIn)
    {
        if (switchedIn.IsAlly)
            chatbox.RefreshMoves(switchedIn);
        else
            enemyUnit.Setup(switchedIn, true);

        yield return PresentSwitch(switchedIn);
    }

    private void BeginPlayerAction(bool immediate = false)
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        if (!order[orderIndex].IsAlly)
        {
            AddMoveCommand(RandomNonNullElement(order[orderIndex].Moves), playerUnit.Pokemon);
        }
        else
        {
            chatbox.SetState(ChatState.SelectAction);
            StartCoroutine(chatbox.Print($"{order[orderIndex].Name}은(는) 무엇을 할까?", immediate));
            BattleState = BattleState.SelectingAction;
        }
    }

    private void BeginPlayerMove()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        chatbox.SetState(ChatState.SelectMove);
        BattleState = BattleState.SelectingMove;
        chatbox.RefreshMoves(order[orderIndex]);

        if (moveIndex >= order[orderIndex].GetFilledMoveSlots())
            moveIndex = 0;

        SetMoveColor(moveIndex, Color.blue);
    }

    private void BeginTurn()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        chatbox.SetState(ChatState.ChatOnly);
        BattleState = BattleState.TurnHappening;
        StartCoroutine(Logic.Turn(pendingMoves, pendingSwitches, usedItems));
    }

    private IEnumerator BeginSwitch()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        BattleState = BattleState.Idle;

        yield return hud.FadeInTransition();
        battleCanvas.SetActive(false);
        partyCanvas.SetActive(true);
        party.Init(this, isForcedSwitch);
        yield return hud.FadeOutTransition();
    }

    private IEnumerator BeginOpenBag()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        BattleState = BattleState.Idle;

        yield return hud.FadeInTransition();
        battleCanvas.SetActive(false);
        bagCanvas.SetActive(true);
        bag.Init(this);
        yield return hud.FadeOutTransition();
    }

    private void MoveToNextInOrder()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        moveIndex = 0;

        if (orderIndex + 1 >= order.Count)
        {
            BeginTurn();
        }
        else
        {
            orderIndex++;
            BeginPlayerAction();
        }
    }

    public void RequestConfirmationBox()
    {
        BattleState = BattleState.Confirming;

        if (chatbox.confirmationObject != null)
            chatbox.confirmationObject.SetActive(true);

        chatbox.ConfirmationBox.CursorYes();
        confirmationIndex = 0;
    }

    public void RequestMoveReplacement(Pokemon learner)
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        chatbox.RefreshMoves(learner);
        chatbox.SetState(ChatState.SelectMove);
        BattleState = BattleState.SelectingReplacedMove;
    }

    public void GoIdle()
    {
        ResetActionTextColors();
        ResetMoveTextColors();

        chatbox.SetState(ChatState.ChatOnly);
        BattleState = BattleState.Idle;
    }

    public void RefreshMoves(Pokemon ally)
    {
        chatbox.RefreshMoves(ally);
    }

    private void ConfirmationPicker()
    {
        var oldConfirmationIndex = confirmationIndex;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            confirmationIndex = confirmationIndex == 1 ? 0 : 1;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            confirmationIndex = confirmationIndex == 0 ? 1 : 0;

        if (oldConfirmationIndex != confirmationIndex)
        {
            PlayChatSound();

            if (confirmationIndex == 0)
                chatbox.ConfirmationBox.CursorYes();
            else
                chatbox.ConfirmationBox.CursorNo();
        }

        if (PressedEsc())
        {
            Logic.Confirmation = false;
            BattleState = BattleState.Idle;

            if (chatbox.confirmationObject != null)
                chatbox.confirmationObject.SetActive(false);

            return;
        }

        if (PressedEnter())
        {
            Logic.Confirmation = confirmationIndex == 0;
            BattleState = BattleState.Idle;

            if (chatbox.confirmationObject != null)
                chatbox.confirmationObject.SetActive(false);
        }
    }

    private void ActionPicker()
    {
        var oldIndex = actionIndex;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            actionIndex = actionIndex < 2 ? actionIndex + 2 : actionIndex - 2;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            actionIndex = (actionIndex + 2) % 4;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            actionIndex = actionIndex % 2 == 0 ? actionIndex + 1 : actionIndex - 1;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            actionIndex = actionIndex % 2 != 0 ? actionIndex - 1 : actionIndex + 1;

        if (oldIndex != actionIndex)
        {
            PlayChatSound();
            SetActionColor(actionIndex, Color.blue);
        }

        if (PressedEnter())
        {
            if (chatbox.IsBusy) return;

            SetActionColor(actionIndex, Color.blue);
            PlayChatSound();

            switch (actionIndex)
            {
                case 0:
                    BeginPlayerMove();
                    break;

                case 1:
                    StartCoroutine(BeginOpenBag());
                    break;

                case 2:
                    isForcedSwitch = false;
                    StartCoroutine(BeginSwitch());
                    break;

                case 3:
                    Logic.TryEscape();
                    AddItemCommand();
                    break;
            }
        }
    }

    private void MovePicker()
    {
        var oldIndex = moveIndex;

        if (Input.GetKeyDown(KeyCode.UpArrow))
            moveIndex = moveIndex < 2 ? moveIndex + 2 : moveIndex - 2;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            moveIndex = (moveIndex + 2) % 4;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            moveIndex = moveIndex % 2 == 0 ? moveIndex + 1 : moveIndex - 1;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            moveIndex = moveIndex % 2 != 0 ? moveIndex - 1 : moveIndex + 1;

        if (chatbox.moves[moveIndex].text == "-")
            moveIndex = oldIndex;

        if (oldIndex != moveIndex)
        {
            PlayChatSound();
            SetMoveColor(moveIndex, Color.blue);
            chatbox.ShowMoveInfo(playerUnit.Moves[moveIndex]);
        }

        if (PressedEsc())
        {
            BackFromMoveSelection();
            return;
        }

        if (PressedEnter())
        {
            if (chatbox.IsBusy) return;

            PlayChatSound();
            SetMoveColor(moveIndex, Color.blue);

            if (BattleState == BattleState.SelectingReplacedMove)
            {
                Logic.MoveLearningSelection = moveIndex;
                return;
            }

            AddMoveCommand(order[orderIndex].Moves[moveIndex], enemyUnit.Pokemon);
        }
    }

    public void PlayHitSound()
    {
        if (hitSound != null) hitSound.Play();
    }

    public void PlayNotVeryEffectiveHitSound()
    {
        if (notVeryEffectiveSound != null) notVeryEffectiveSound.Play();
    }

    public void PlaySuperEffectiveHitSound()
    {
        if (superEffectiveSound != null) superEffectiveSound.Play();
    }

    public void PlayLevelUpSound()
    {
        if (levelUpSound != null) levelUpSound.Play();
    }
}