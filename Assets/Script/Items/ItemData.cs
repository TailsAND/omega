using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class ItemData {
    public PickItemType Type;
    public List<SkillType> Skills = new List<SkillType>();
    public string configId;
    public ItemRank rank;
    public bool IsUsable => Type==PickItemType.Apple || Type==PickItemType.ManaPotion;
    
    public int healthBonus;
    public int armorBonus;
    public int attackBonus;
    public SpecialStatType[] specialStats;
    public float[] specialStatsValues;
    public int level;
}

public enum PickItemType {
    Apple,ManaPotion,Boots,Staff, Chestplate,ingredients
}
public static class ItemConfigExtensions
{
    public static void CopyFrom(this EquipmentItemData target, ItemConfig source)
    {
        if (target == null || source == null)
            return;

        // Basic Settings
        target.itemName = source.itemName;
        target.Description = source.Description;
        target.icon = source.icon;
        target.itemType = source.itemType;
        target.Rank = source.Rank;
        target.rarity = source.rarity;
        target.Prefab = source.Prefab;
        target.PlayerPrefsName = source.PlayerPrefsName;
        target.canBeStolen = source.canBeStolen;
        target.Coins = source.Coins;
        target.time = source.time;

        // Weapon Settings
        target.weaponType = source.weaponType;
        target.weaponRange = source.weaponRange;
        target.attackSpeed = source.attackSpeed;
        target.baseDamage = source.baseDamage;
        target.isHealing = source.isHealing;
        target.HealingPower = source.HealingPower;
        target.isMana = source.isMana;
        target.ManaPower = source.ManaPower;

        // Skills
        
        target.Skills = source.Skills?.Select(s => s.ToString()).ToList() ?? new List<string>();

        // Stat Ranges
        target.dRankStats = new ItemConfig.RankStatRanges
        {
            health = source.dRankStats.health,
            attack = source.dRankStats.attack,
            armor = source.dRankStats.armor,
            perLevelIncrease = source.dRankStats.perLevelIncrease,
            possibleSpecialStats = (SpecialStatType[])source.dRankStats.possibleSpecialStats?.Clone(),
            specialStatRanges = (Vector2[])source.dRankStats.specialStatRanges?.Clone()
        };

        target.cRankStats = new ItemConfig.RankStatRanges
        {
            health = source.cRankStats.health,
            attack = source.cRankStats.attack,
            armor = source.cRankStats.armor,
            perLevelIncrease = source.cRankStats.perLevelIncrease,
            possibleSpecialStats = (SpecialStatType[])source.cRankStats.possibleSpecialStats?.Clone(),
            specialStatRanges = (Vector2[])source.cRankStats.specialStatRanges?.Clone()
        };

        target.bRankStats = new ItemConfig.RankStatRanges
        {
            health = source.bRankStats.health,
            attack = source.bRankStats.attack,
            armor = source.bRankStats.armor,
            perLevelIncrease = source.bRankStats.perLevelIncrease,
            possibleSpecialStats = (SpecialStatType[])source.bRankStats.possibleSpecialStats?.Clone(),
            specialStatRanges = (Vector2[])source.bRankStats.specialStatRanges?.Clone()
        };

        target.aRankStats = new ItemConfig.RankStatRanges
        {
            health = source.aRankStats.health,
            attack = source.aRankStats.attack,
            armor = source.aRankStats.armor,
            perLevelIncrease = source.aRankStats.perLevelIncrease,
            possibleSpecialStats = (SpecialStatType[])source.aRankStats.possibleSpecialStats?.Clone(),
            specialStatRanges = (Vector2[])source.aRankStats.specialStatRanges?.Clone()
        };

        target.sRankStats = new ItemConfig.RankStatRanges
        {
            health = source.sRankStats.health,
            attack = source.sRankStats.attack,
            armor = source.sRankStats.armor,
            perLevelIncrease = source.sRankStats.perLevelIncrease,
            possibleSpecialStats = (SpecialStatType[])source.sRankStats.possibleSpecialStats?.Clone(),
            specialStatRanges = (Vector2[])source.sRankStats.specialStatRanges?.Clone()
        };
    }
}