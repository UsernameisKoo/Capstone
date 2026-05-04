using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Utils;
using System;

public abstract class MoveFunctions
{
    public abstract IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount);

    public virtual IEnumerator OnUse(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount) { yield break; }
    public virtual IEnumerator OnHit(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount) { yield break; }
    public virtual IEnumerator OnMiss(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount) { yield break; }
    public virtual IEnumerator OnOverworld() { yield break; }
}

public enum MoveLogic
{
    Struggle, Scratch, Tackle, VineWhip, BlazeKick, Blizzard, Ember, Growl, TailWhip, SandAttack, Growth, RazorLeaf, PoisonPowder, SleepPowder, StunSpore,
    LeechSeed
}

public enum Targeting
{
    Single, Self, Adjacent, Allies, Enemies, All
}

public class Struggle : MoveFunctions
{
    private int damage;

    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        damage = CalcDamage(move, user, target, battle, targetCount);
        target.Health -= damage;
        yield break;
    }

    public override IEnumerator OnHit(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        yield return battle.Print($"{user.Name}은(는) 반동으로 데미지를 입었다!");
        var recoil = Mathf.FloorToInt(damage * 0.25f);
        user.Health -= recoil < 1 ? 1 : recoil;
    }
}

public class Scratch : MoveFunctions // 할퀴기
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        yield break;
    }
}

public class Tackle : MoveFunctions // 몸통박치기
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        yield break;
    }
}

public class VineWhip : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        yield break;
    }
}

public class BlazeKick : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        user.CritStage += 2;
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        user.CritStage -= 2;

        if (target.Status == Status.None && Chance(10))
            yield return battle.Logic.AddEffect(EffectLogic.Burn, user, target);
    }
}

public class Blizzard : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);

        if (target.Status == Status.None && Chance(10))
            yield return battle.Logic.AddEffect(EffectLogic.Freeze, user, target);
    }
}

public class Ember : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);

        if (target.Status == Status.None && Chance(10))
            yield return battle.Logic.AddEffect(EffectLogic.Burn, user, target);
    }
}

public class Growl : MoveFunctions // 울음소리
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.AttackStage--;
        yield return battle.Print($"{target.Name}의 공격이 떨어졌다!");
    }
}

public class TailWhip : MoveFunctions // 꼬리치기
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.DefenseStage--;
        yield return battle.Print($"{target.Name}의 방어가 떨어졌다!");
    }
}

public class SandAttack : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.AccuracyStage--;
        yield return battle.Print($"{target.Name}의 명중률이 떨어졌다!");
    }
}

public class Growth : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        if (battle.Logic.Weather == Weather.Sunny)
        {
            user.AttackStage += 2;
            user.SpAttackStage += 2;
            yield return battle.Print($"{user.Name}의 힘이 크게 상승했다!");
        }
        else
        {
            user.AttackStage++;
            user.SpAttackStage++;
            yield return battle.Print($"{user.Name}의 힘이 상승했다!");
        }
    }
}

public class RazorLeaf : MoveFunctions // 할퀴기 (변경 반영)
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        user.CritStage += 2;
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        user.CritStage -= 2;
        yield break;
    }
}

public class PoisonPowder : MoveFunctions // 꼬리치기 (변경 반영)
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.DefenseStage--;
        yield return battle.Print($"{target.Name}의 방어가 떨어졌다!");
    }
}

public class SleepPowder : MoveFunctions // 연속치기 (변경 반영)
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        int hits = UnityEngine.Random.Range(2, 5);
        int totalDamage = 0;

        for (int i = 0; i < hits; i++)
        {
            int damage = CalcDamage(move, user, target, battle, targetCount);
            target.Health -= damage;
            totalDamage += damage;
        }

        yield return battle.Print($"{hits}번 연속으로 공격했다!");
    }
}

public class StunSpore : MoveFunctions
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        if (target.Status == Status.None && !target.IsType(Type.Grass) && !target.IsType(Type.Electric))
            yield return battle.Logic.AddEffect(EffectLogic.Paralysis, user, target);
        else
            yield return battle.Print("하지만 실패했다!");
    }
}

public class LeechSeed : MoveFunctions // 박치기 (변경 반영)
{
    public override IEnumerator Execute(Move move, Pokemon user, Pokemon target, IBattle battle, int targetCount)
    {
        target.Health -= CalcDamage(move, user, target, battle, targetCount);
        yield break;
    }
}