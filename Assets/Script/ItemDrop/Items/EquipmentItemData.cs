using UnityEngine;
using System;
using System.Collections.Generic;

public class EquipmentItemData : ItemConfig
{
    public ItemConfig Config { get; private set; }
    public List<string> Skills { get;  set; }
    private ItemRank Rank { get ;  set; }
    public int Level { get; private set; }
    public int Health { get; private set; }
    public int Armor { get; private set; }
    public int Attack { get; private set; }
    public SpecialStatType[] SpecialStats { get; private set; }
    public float[] SpecialStatsValues { get; private set; }

    public void Initialize(ItemConfig config, TestenemyHealth enemyHealth)
    {
        Config = config;
        Rank = DetermineRankByEnemyLevel(enemyHealth.Level);
        Generate(Rank, enemyHealth);
    }
    // public static EquipmentItemData CreateFromConfig(ItemConfig source)
    // {
    //     var item = ScriptableObject.CreateInstance<EquipmentItemData>();
    //     item.Initialize(source, someEnemyHealth); // Если требуется инициализация
    //     // Нельзя напрямую устанавливать приватные свойства
    //     return item;
    // }
    private ItemRank DetermineRankByEnemyLevel(int enemyLevel)
    {
        Dictionary<ItemRank, float> rankChances = GetRankChancesForEnemyLevel(enemyLevel);
        
        float randomValue = UnityEngine.Random.Range(0f, 100f);
        float cumulativeChance = 0f;

        foreach (var kvp in rankChances)
        {
            cumulativeChance += kvp.Value;
            if (randomValue <= cumulativeChance)
            {
                return kvp.Key;
            }
        }
        
        return ItemRank.D;
    }

    private Dictionary<ItemRank, float> GetRankChancesForEnemyLevel(int enemyLevel)
    {
        var chances = new Dictionary<ItemRank, float>();
        
        if (enemyLevel <= 5)
        {
            chances[ItemRank.D] = 100f;
        }
        else if (enemyLevel <= 10)
        {
            chances[ItemRank.D] = 80f;
            chances[ItemRank.C] = 20f;
        }
        else if (enemyLevel <= 15)
        {
            chances[ItemRank.D] = 60f;
            chances[ItemRank.C] = 35f;
            chances[ItemRank.B] = 5f;
        }
        else if (enemyLevel <= 20)
        {
            chances[ItemRank.D] = 40f;
            chances[ItemRank.C] = 45f;
            chances[ItemRank.B] = 14f;
            chances[ItemRank.A] = 1f;
        }
        else if (enemyLevel <= 25)
        {
            chances[ItemRank.D] = 30f;
            chances[ItemRank.C] = 40f;
            chances[ItemRank.B] = 25f;
            chances[ItemRank.A] = 4.5f;
            chances[ItemRank.S] = 0.5f;
        }
        else 
        {
            chances[ItemRank.D] = 20f;
            chances[ItemRank.C] = 30f;
            chances[ItemRank.B] = 35f;
            chances[ItemRank.A] = 12f;
            chances[ItemRank.S] = 3f;
        }

        return chances;
    }

    public void Generate(ItemRank rank, TestenemyHealth enemyHealth)
    {
        Rank = rank;
        Level = CalculateItemLevel(enemyHealth);
        
        var ranges = Config.GetRangesForRank(rank);
        int levelBonus = (Level - 1) * ranges.perLevelIncrease;
        
        Health = CalculateStat(ranges.health, levelBonus, 50);
        Armor = CalculateStat(ranges.armor, levelBonus, 5);
        Attack = CalculateStat(ranges.attack, levelBonus, 5);
        
        Skills = SkillsTable.Instance?.GetSkillsForItem() ?? new List<string>();
        GenerateSpecialStats();
        
        LogStats();
    }

    private int CalculateItemLevel(TestenemyHealth enemyHealth)
    {
        
        return enemyHealth.Level + UnityEngine.Random.Range(0, 3);
    }

    private int CalculateStat(Vector2Int range, int levelBonus, int maxRandomBonus)
    {
        return UnityEngine.Random.Range(
            range.x + levelBonus,
            range.y + levelBonus + UnityEngine.Random.Range(0, maxRandomBonus + 1)
        );
    }
    
    public ItemData GenerateItemData(TestenemyHealth enemyHealth)
    {
        Initialize(this, enemyHealth);
        
        var itemData = new ItemData {
            configId = this.name,
            rank = this.Rank,
            healthBonus = this.Health,
            armorBonus = this.Armor,
            attackBonus = this.Attack,
            specialStats = this.SpecialStats,
            specialStatsValues = this.SpecialStatsValues,
            level = this.Level,
            Type = ConvertItemTypeToPickType(this.itemType)
        };
        
        return itemData;
    }
    
    private PickItemType ConvertItemTypeToPickType(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Weapon => PickItemType.Staff, // или другой тип оружия
            ItemType.Armor => PickItemType.Chestplate,
            // ... другие соответствия ...
            _ => PickItemType.ingredients
        };
    }
    
    private void GenerateSpecialStats()
    {
        int statsCount = Rank switch
        {
            ItemRank.D => 0,
            ItemRank.C or ItemRank.B => 1,
            ItemRank.A or ItemRank.S => 2,
            _ => 0
        };

        SpecialStats = new SpecialStatType[statsCount];
        SpecialStatsValues = new float[statsCount];

        for (int i = 0; i < statsCount; i++)
        {
            SpecialStats[i] = GetRandomSpecialStat();
            SpecialStatsValues[i] = CalculateSpecialStatValue(SpecialStats[i]);
        }
    }

    private SpecialStatType GetRandomSpecialStat()
    {
        Array values = Enum.GetValues(typeof(SpecialStatType));
        return (SpecialStatType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    private float CalculateSpecialStatValue(SpecialStatType stat)
    {
        float baseValue = stat switch
        {
            SpecialStatType.Dodge => UnityEngine.Random.Range(1f, 3f),
            SpecialStatType.CRIT => UnityEngine.Random.Range(2f, 5f),
            SpecialStatType.DodgeRES => UnityEngine.Random.Range(1f, 2.5f),
            SpecialStatType.CRITRES => UnityEngine.Random.Range(1.5f, 3f),
            SpecialStatType.HPSteel => UnityEngine.Random.Range(0.5f, 1.5f),
            SpecialStatType.BoostCRITDMG => UnityEngine.Random.Range(3f, 7f),
            SpecialStatType.ReduceCRITDMG => UnityEngine.Random.Range(2f, 5f),
            SpecialStatType.RestoreHP => UnityEngine.Random.Range(1f, 3f),
            SpecialStatType.BoostDMG => UnityEngine.Random.Range(2f, 4f),
            _ => 0f
        };

        return baseValue * (1 + (Level * 0.05f));
    }

    public void LogStats()
    {
        string log = $"[{Rank}] {Config.itemName} (Lvl {Level})\n" +
                    $"HP: {Health} | Armor: {Armor} | ATK: {Attack}\n";
        
        if (SpecialStats.Length > 0)
        {
            log += "Special Stats:\n";
            for (int i = 0; i < SpecialStats.Length; i++)
            {
                log += $"- {SpecialStats[i]}: {SpecialStatsValues[i]:F1}\n";
            }
        }

        Debug.Log(log);
    }
}