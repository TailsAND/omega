using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerEquipment : NetworkBehaviour {
    private Dictionary<ItemType, ItemConfig> _playerInventoryConfig = new();
    private Dictionary<ItemType, ItemData> _playerInventoryData = new();
    private Dictionary<ItemType, Image> _itemTypeToImageMap = new();

    public static PlayerEquipment Instance;
    private PlayerStats _playerStats;
    
    [SerializeField] private Image imageForArmor;
    [SerializeField] private Image imageForAccessory;
    [SerializeField] private Image imageForWeapon;
    [SerializeField] private Image imageForBoots;
    [SerializeField] private Image imageForHelmet;
    [SerializeField] private Image imageForShoulderPads;
    [SerializeField] private Image imageForChestPlate;
    [SerializeField] private Image imageForBracers;
    [SerializeField] private Image imageForNecklace;
    [SerializeField] private Image imageForBracelet;


    private void Awake()
    {
        Instance = this;
        InitializeItemTypeMap();
        _playerStats = GetComponent<PlayerStats>();
    }

    public Dictionary<ItemType, ItemConfig> GetAllItems() {
        return _playerInventoryConfig;
    }

    public ItemConfig GetItemConfig(ItemType itemType) {
        return _playerInventoryConfig[itemType];
    }

    public ItemData GetItemData(ItemType itemType) {
        return _playerInventoryData[itemType];
    }

    public void WearItem(ItemConfig equipmentItemConfig, ItemData itemData)
    {
        if (equipmentItemConfig == null || itemData == null) return;

        // Применяем характеристики предмета
        ApplyItemStats(equipmentItemConfig, itemData, true);
        
        if (equipmentItemConfig.Skills.Count < 0)
        {
            SetEquipmentImage(equipmentItemConfig);
            return;
        }

        var skillController = InventoryManager.Instance.PlayerSkillController;
        skillController.SkillManager.Skills.Clear();

        SetEquipment(equipmentItemConfig, itemData);

        GameUI.Instance.SkillContainerView.gameObject.GetComponent<SkillSelectorHandler>().UpdateSkillSelector();
        SetEquipmentImage(equipmentItemConfig);
    }

    private void SetEquipment(ItemConfig equipmentItemConfig, ItemData itemData) {
        _playerInventoryConfig[equipmentItemConfig.itemType] = equipmentItemConfig;
        _playerInventoryData[equipmentItemConfig.itemType] = itemData;
        Debug.Log(_playerInventoryConfig[equipmentItemConfig.itemType]);
    }

    public void Unwear(ItemConfig equipmentItemConfig, ItemData itemData)
    {
        if (equipmentItemConfig == null || itemData == null) return;

        // Удаляем характеристики предмета
        ApplyItemStats(equipmentItemConfig, itemData, false);
        InventoryManager.Instance.PlayerSkillController.gameObject.GetComponent<PlayerInventory>()
            .PutInEmptySlot(equipmentItemConfig, itemData);

        DeleteEquipment(equipmentItemConfig);

        foreach (var skillType in itemData.Skills)
        {
            InventoryManager.Instance.PlayerSkillController.DeleteSkill(
                SkillFactory.Create(ConfigsManager.GetSkillConfig(skillType),
                    InventoryManager.Instance.PlayerSkillController));
        }

        GameUI.Instance.button.Disable();
        UnsetEquipmentImage(equipmentItemConfig);

        InventoryManager.Instance.PlayerSkillController.AddNewSkillFromItem();
        GameUI.Instance.SkillContainerView.gameObject.GetComponent<SkillSelectorHandler>().UpdateSkillSelector();
    }
    
    private void ApplyItemStats(ItemConfig itemConfig, ItemData itemData, bool isEquipping)
    {


        if (itemConfig == null || itemData == null) return;

        int multiplier = isEquipping ? 1 : -1;

        

        // Применяем базовые характеристики
        InventoryManager.Instance.PlayerSkillController.gameObject.GetComponent<PlayerStats>().MaxHp += itemData.healthBonus * multiplier;
        InventoryManager.Instance.PlayerSkillController.gameObject.GetComponent<PlayerStats>().Armor += itemData.armorBonus * multiplier;
        
        PlayerUI.Instance.UpdateUI();


        // Если это оружие, обновляем характеристики атаки
        if (itemConfig.weaponType != WeaponType.None)
        {
            var combat = GetComponent<PlayerCombat>();
            if (combat != null)
            {
                // Можно добавить модификаторы атаки здесь
            }
        }

        // Применяем специальные характеристики
        if (itemData.specialStats != null)
        {
            for (int i = 0; i < itemData.specialStats.Length; i++)
            {
                ApplySpecialStat(itemData.specialStats[i], itemData.specialStatsValues[i] * multiplier);
            }
        }
    }
    
    
    private void ApplySpecialStat(SpecialStatType statType, float value)
    {
        // Реализация специальных эффектов
        switch (statType)
        {
            case SpecialStatType.CRIT:
                // Увеличиваем шанс крита
                break;
            case SpecialStatType.Dodge:
                // Увеличиваем шанс уклонения
                break;
            // ... другие специальные характеристики ...
        }
    }
    
    private void DeleteEquipment(ItemConfig equipmentItemConfig) {
        _playerInventoryConfig.Remove(equipmentItemConfig.itemType);
        _playerInventoryData.Remove(equipmentItemConfig.itemType);
    }

    private void UnsetEquipmentImage(ItemConfig equipmentItemConfig) {
        if (_itemTypeToImageMap.TryGetValue(equipmentItemConfig.itemType, out var image)) {
            UnSetImage(image, equipmentItemConfig.icon);
        }
    }

    private void SetEquipmentImage(ItemConfig equipmentItemConfig) {
        if (_itemTypeToImageMap.TryGetValue(equipmentItemConfig.itemType, out var image)) {
            SetImage(image, equipmentItemConfig.icon);
        }
        else {
            Debug.LogError("not correct item type or no type: " + equipmentItemConfig.itemType);
        }

        GameUI.Instance.button.SetData(_playerInventoryConfig[equipmentItemConfig.itemType],
            _playerInventoryData[equipmentItemConfig.itemType], Unwear);
    }

    private void SetImage(Image image, Sprite sprite) {
        image.gameObject.SetActive(true);
        image.sprite = sprite;
    }

    private void UnSetImage(Image image, Sprite sprite) {
        image.gameObject.SetActive(false);
        image.sprite = null;
    }

    private void InitializeItemTypeMap() {
        _itemTypeToImageMap = new Dictionary<ItemType, Image>() {
            { ItemType.Armor, imageForArmor },
            { ItemType.Accessory, imageForAccessory },
            { ItemType.Weapon, imageForWeapon },
            { ItemType.weapon, imageForWeapon },
            { ItemType.boots, imageForBoots },
            { ItemType.bracelet, imageForBracelet },
            { ItemType.chestPlate, imageForChestPlate },
            { ItemType.necklace, imageForNecklace },
            { ItemType.shoulderPads, imageForShoulderPads },
            { ItemType.bracers, imageForBracers },
            { ItemType.helmet, imageForHelmet }
        };
    }
}