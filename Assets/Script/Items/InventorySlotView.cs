using UnityEngine;
using UnityEngine.UI;

public class InventorySlotView : MonoBehaviour {
    [SerializeField]
    private Image _icon; // Иконка предмета
    [SerializeField]
    private Image _slotBackground; // Фон слота
    [SerializeField]
    private Button _button;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = new Color(0.7f, 0.7f, 0.7f); // Серый
    [SerializeField] private Color uncommonColor = new Color(0.2f, 0.8f, 0.2f); // Зеленый
    [SerializeField] private Color rareColor = new Color(0.2f, 0.4f, 1f); // Синий
    [SerializeField] private Color epicColor = new Color(0.6f, 0.2f, 0.8f); // Фиолетовый
    [SerializeField] private Color legendaryColor = new Color(1f, 0.5f, 0f); // Оранжевый

    [HideInInspector]
    public ItemConfig slotItemConfig;

    [HideInInspector]
    public ItemData slotItemData;

    private void Start() {
        InitButton();
    }

    private void InitButton() {
        _button.onClick.AddListener(ShowInfo);
    }

    public void PutInSlot(ItemConfig itemConfig, ItemData itemData) {
        // Проверяем, не надет ли уже этот предмет
        bool isEquipped = PlayerEquipment.Instance.GetAllItems().ContainsKey(itemConfig.itemType) && 
                          PlayerEquipment.Instance.GetItemConfig(itemConfig.itemType) == itemConfig;

        if (!isEquipped) {
            _icon.sprite = itemConfig.icon;
            slotItemConfig = itemConfig;
            slotItemData = itemData;
            _icon.enabled = true;
    
            // Устанавливаем цвет фона слота в зависимости от редкости
            SetSlotColor(itemConfig.rarity);
    
            LogItemInfo(itemConfig, itemData);
        } else {
            ClearSlot();
        }
    }

    private void SetSlotColor(ItemRarity rarity) {
        switch (rarity) {
            case ItemRarity.Common:
                _slotBackground.color = commonColor;
                break;
            case ItemRarity.Uncommon:
                _slotBackground.color = uncommonColor;
                break;
            case ItemRarity.Rare:
                _slotBackground.color = rareColor;
                break;
            case ItemRarity.Epic:
                _slotBackground.color = epicColor;
                break;
            case ItemRarity.Legendary:
                _slotBackground.color = legendaryColor;
                break;
            default:
                _slotBackground.color = commonColor;
                break;
        }
    }

    private void LogItemInfo(ItemConfig itemConfig, ItemData data) {

        var rankStats = itemConfig.GetRangesForRank(itemConfig.Rank);
        

        string specialStatsStr = "";
        if (rankStats.possibleSpecialStats != null && rankStats.specialStatRanges != null)
        {
            for (int i = 0; i < Mathf.Min(rankStats.possibleSpecialStats.Length, rankStats.specialStatRanges.Length); i++)
            {
                specialStatsStr += $"\n{rankStats.possibleSpecialStats[i]}: {rankStats.specialStatRanges[i].x}-{rankStats.specialStatRanges[i].y}";
            }
        }

        Debug.Log($"=== Item Info ===\n" +
                  $"Name: {itemConfig.itemName}\n" +
                  $"Type: {itemConfig.itemType}\n" +
                  $"Rarity: {itemConfig.rarity}\n" +
                  $"Rank: {itemConfig.Rank}\n" +
                  $"\n=== Base Stats ===\n" +
                  $"Health: {rankStats.health.x}-{rankStats.health.y}\n" +
                  $"Attack: {rankStats.attack.x}-{rankStats.attack.y}\n" +
                  $"Armor: {rankStats.armor.x}-{rankStats.armor.y}\n" +
                  $"Per Level Increase: {rankStats.perLevelIncrease}\n" +
                  $"\n=== Special Stats ==={specialStatsStr}\n" +
                  $"\n=== Other Properties ===\n" +
                  $"Healing: {(itemConfig.isHealing ? itemConfig.HealingPower.ToString() : "No")}\n" +
                  $"Mana: {(itemConfig.isMana ? itemConfig.ManaPower.ToString() : "No")}\n" +
                  $"Coins: {itemConfig.Coins}\n" +
                  $"Time: {itemConfig.time}\n" +
                  $"=========================");
    }

    void ShowInfo() {
        if (slotItemConfig != null) {
            ItemInfo.instance.Open(slotItemConfig, slotItemData, this);
        }
    }

    public void ClearSlot() {
        slotItemConfig = null;
        slotItemData = null;
        _icon.sprite = null;
        _icon.enabled = false;
        _slotBackground.color = commonColor;    
    }
}