using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Pokemon", menuName = "Pokemon")]

/// <summary>
/// The skeleton data for a Pokemon, which can be set through the Unity UI.
/// </summary>
public class PokemonBase : ScriptableObject
{
    public string pokemonName;
    public int dexNumber; // also used to find the correct animations for front and back

    [Header("기본 정보")]
    public Sprite icon;

    [Header("전투용 스프라이트")]
    public Sprite frontSprite;   // 상대 포켓몬용 앞모습
    public Sprite backSprite;    // 내 포켓몬용 뒷모습

    public Type primaryType;
    public Type secondaryType;

    [Header("스탯")]
    public int hpStat;
    public int atkStat;
    public int defStat;
    public int spAtkStat;
    public int spDefStat;
    public int spdStat;

    public ExpGroup expGroup;
    public int catchRate;
    public int expStat; // the base xp yield
    public float maleChance; // 0-100 chance of male. -1 = genderless

    [Header("사운드")]
    public AudioClip cry;

    [Header("진화")]
    public LevelEvolution levelEvolution;
    public ItemEvolution[] itemEvolutions;

    [Header("기술 / 특성")]
    public AbilityBase[] learnableAbilities;
    public LearnableMove[] learnableMoves;
}

[System.Serializable]
public class LearnableMove
{
    public MoveBase skeleton;
    public int level;
}

[System.Serializable]
public class LevelEvolution
{
    public PokemonBase evolution;
    public int level;
}

[System.Serializable]
public class ItemEvolution
{
    public PokemonBase evolution;
    public ItemBase item;
}

public enum ExpGroup
{
    Erratic, Fast, MediumFast, MediumSlow, Slow, Fluctuating
}