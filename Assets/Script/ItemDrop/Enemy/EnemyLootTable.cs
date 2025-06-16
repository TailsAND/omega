using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyLootTable", menuName = "Loot/EnemyLootTable")]
public class EnemyLootTable : ScriptableObject
{
    [System.Serializable]
    public class RankDropSettings
    {
        public ItemRank rank;
        [Range(0f, 100f)] public float baseChance;
        public float commonModifier = 0.5f;
        public float eliteModifier = 1f;
        public float championModifier = 1.5f;
        public float bossModifier = 2f;
        public float legendaryModifier = 3f;
    }

    public ItemConfig[] possibleItemConfigs;
    public RankDropSettings[] rankChances;

    public EquipmentItemData GetRandomItem(EnemyRank enemyRank, int playerLevel, TestenemyHealth enemyHealth)
    {
        if (possibleItemConfigs == null || possibleItemConfigs.Length == 0)
        {
            Debug.LogWarning("No item configs available in loot table");
            return null;
        }

        ItemRank itemRank = DetermineItemRank(enemyRank);
        ItemConfig config = GetRandomItemOfRank(itemRank);

        if (config != null) 
        {
            EquipmentItemData item = ScriptableObject.CreateInstance<EquipmentItemData>();
            item.CopyFrom(config);
            item.Initialize(config, enemyHealth);
            return item;
        }

        return null;
    }

    private ItemRank DetermineItemRank(EnemyRank enemyRank)
    {
        float totalWeight = rankChances.Sum(setting => GetModifiedChance(setting, enemyRank));
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var setting in rankChances)
        {
            currentWeight += GetModifiedChance(setting, enemyRank);
            if (randomValue <= currentWeight)
            {
                return setting.rank;
            }
        }

        return ItemRank.D;
    }

    private float GetModifiedChance(RankDropSettings setting, EnemyRank enemyRank)
    {
        return setting.baseChance * (enemyRank switch
        {
            EnemyRank.Common => setting.commonModifier,
            EnemyRank.Elite => setting.eliteModifier,
            EnemyRank.Champion => setting.championModifier,
            EnemyRank.Boss => setting.bossModifier,
            EnemyRank.Legendary => setting.legendaryModifier,
            _ => 1f
        });
    }

    private ItemConfig GetRandomItemOfRank(ItemRank rank)
    {
        var availableConfigs = possibleItemConfigs.Where(config => config != null && config.Rank == rank).ToArray();
        return availableConfigs.Length == 0 ? null : availableConfigs[Random.Range(0, availableConfigs.Length)];
    }
}