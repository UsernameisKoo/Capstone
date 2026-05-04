using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;
using System;

public abstract class EffectFunctions
{
    public string Name { get; set; }
    public int? Duration { get; set; }
    public Trigger Trigger { get; set; }
    public bool EndOnSwitch { get; set; }

    public virtual IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle) { yield return null; }
    public virtual IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle) { yield return null; }
    public virtual IEnumerator OnDeletion(Effect effect, Pokemon user, Pokemon target, IBattle battle) { yield return null; }
    public virtual IEnumerator OnSwitchOut(Effect effect, Pokemon user, Pokemon target, IBattle battle) { yield return null; }
    public virtual IEnumerator OnOverworld() { yield return null; }
}

public enum EffectLogic
{
    Freeze, Burn, Poison, ToxicPoison, Sleep, Paralysis, LeechSeeded
}

public enum Trigger
{
    StartOfTurn, EndOfTurn, OnDeath, OnSwitchIn, OnSwitchOut
}

public class Freeze : EffectFunctions
{
    public Freeze()
    {
        Name = "얼음";
        Duration = RandomInt(1, 4);
        Trigger = Trigger.StartOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Frozen;
        target.CanAttack = false;
        yield return battle.Print($"{target.Name}은(는) 얼어붙었다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 꽁꽁 얼어 있다!");
    }

    public override IEnumerator OnDeletion(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.None;
        target.CanAttack = true;
        yield return battle.Print($"{target.Name}은(는) 얼음이 녹았다!");
    }
}

public class Burn : EffectFunctions
{
    public Burn()
    {
        Name = "화상";
        Trigger = Trigger.EndOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Burned;
        yield return battle.Print($"{target.Name}은(는) 화상을 입었다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 화상으로 고통받고 있다...");
        var damage = Mathf.FloorToInt(target.MaxHealth / 16f);
        target.Health -= damage < 1 ? 1 : damage;
    }
}

public class Poison : EffectFunctions
{
    public Poison()
    {
        Name = "독";
        Trigger = Trigger.EndOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Poisoned;
        yield return battle.Print($"{target.Name}은(는) 독에 중독되었다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 독 때문에 데미지를 입고 있다...");
        var damage = Mathf.FloorToInt(target.MaxHealth / 16f);
        target.Health -= damage < 1 ? 1 : damage;
    }
}

public class ToxicPoison : EffectFunctions
{
    public ToxicPoison()
    {
        Name = "맹독";
        Trigger = Trigger.EndOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Toxic;
        yield return battle.Print($"{target.Name}은(는) 맹독에 중독되었다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 맹독 때문에 심하게 데미지를 입고 있다...");
        var damage = Mathf.FloorToInt(target.MaxHealth / (16f / effect.Turn));
        target.Health -= damage < 1 ? 1 : damage;
    }

    public override IEnumerator OnSwitchOut(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        effect.Turn = 1;
        yield break;
    }
}

public class Sleep : EffectFunctions
{
    public Sleep()
    {
        Name = "수면";
        Duration = RandomInt(1, 4);
        Trigger = Trigger.StartOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Sleeping;
        target.CanAttack = false;
        yield return battle.Print($"{target.Name}은(는) 잠들어버렸다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 자고 있다...");
    }

    public override IEnumerator OnDeletion(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.None;
        target.CanAttack = true;
        yield return battle.Print($"{target.Name}은(는) 눈을 떴다!");
    }
}

public class Paralysis : EffectFunctions
{
    public Paralysis()
    {
        Name = "마비";
        Trigger = Trigger.StartOfTurn;
        EndOnSwitch = false;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.Paralyzed;
        target.CanAttack = Chance(75);
        yield return battle.Print($"{target.Name}은(는) 마비되었다!");
        if (!target.CanAttack) yield return battle.Print($"{target.Name}은(는) 몸이 마비되어 움직일 수 없다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.CanAttack = Chance(75);
        if (!target.CanAttack) yield return battle.Print($"{target.Name}은(는) 몸이 마비되어 움직일 수 없다!");
    }

    public override IEnumerator OnDeletion(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        target.Status = Status.None;
        target.CanAttack = true;
        yield return battle.Print($"{target.Name}은(는) 마비에서 회복되었다!");
    }
}

public class LeechSeeded : EffectFunctions
{
    public LeechSeeded()
    {
        Name = "씨뿌리기";
        Trigger = Trigger.EndOfTurn;
        EndOnSwitch = true;
    }

    public override IEnumerator OnCreation(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        yield return battle.Print($"{target.Name}은(는) 씨앗이 심어졌다!");
    }

    public override IEnumerator Execute(Effect effect, Pokemon user, Pokemon target, IBattle battle)
    {
        if (battle.Logic.ActiveAllies.Contains(user) && user.Health > 0)
        {
            var sapped = Limit(1, Mathf.FloorToInt(target.Health * 0.125f), target.Health);
            target.Health -= sapped;
            user.Health += sapped;
            yield return battle.Print($"{user.Name}은(는) {target.Name}의 체력을 흡수했다!");
        }
    }
}