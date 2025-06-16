using System;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerInventory : NetworkBehaviour {
    public List<InventorySlot> Slots = new List<InventorySlot>(24);
    public event Action OnInventoryChanged;

public bool PutInEmptySlot(ItemConfig itemConfig, ItemData itemData) 
{
    for (int i = 0; i < Slots.Count; i++) 
    {
        if (Slots[i].ItemConfig == null) 
        {
            Slots[i] = new InventorySlot {
                ItemConfig = itemConfig,
                ItemData = itemData
            };
        
            if (Slots[i].SlotView != null) 
            {
                Slots[i].SlotView.PutInSlot(itemConfig, itemData);
            }
            
            OnInventoryChanged?.Invoke();
            return true;
        }
    }
    return false;
}

[Server]
public void AddLoot(EquipmentItemData itemData, TestenemyHealth enemyHealth)
{
    if (!isServer) return;

    var generatedItemData = itemData.GenerateItemData(enemyHealth);
    var itemConfig = ItemDatabase.GetItemById(itemData.name);
        
    if (PutInEmptySlot(itemConfig, generatedItemData))
    {
        Debug.Log($"Added loot: {itemConfig.itemName} with stats: HP+{generatedItemData.healthBonus}, Armor+{generatedItemData.armorBonus}");
    }
}

    [Server]
    public ItemData StealRandomItem()
    {
        if (!isServer) return null;

        // Собираем все непустые слоты с предметами, которые можно украсть
        List<InventorySlot> stealableSlots = new List<InventorySlot>();
        foreach (var slot in Slots)
        {
            if (slot.ItemConfig != null && slot.ItemConfig.canBeStolen)
            {
                stealableSlots.Add(slot);
            }
        }

        // Если нет предметов для кражи
        if (stealableSlots.Count == 0)
        {
            Debug.Log("No items to steal in inventory");
            return null;
        }

        // Выбираем случайный слот для кражи
        int randomIndex = Random.Range(0, stealableSlots.Count);
        InventorySlot stolenSlot = stealableSlots[randomIndex];
    
        // Создаем копию данных предмета перед удалением
        ItemData stolenItemData = new ItemData
        {
            configId = stolenSlot.ItemConfig.name,
            rank = stolenSlot.ItemData?.rank ?? ItemRank.C
        };

        // Удаляем предмет из инвентаря
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i] == stolenSlot)
            {
                if (Slots[i].SlotView != null)
                {
                    Slots[i].SlotView.ClearSlot();
                }
                Slots[i] = new InventorySlot();
                break;
            }
        }

        // Вызываем событие обновления инвентаря
        RpcUpdateInventory();
    
        Debug.Log($"Stolen item: {stolenItemData.configId}");
    
        return stolenItemData;
    }

    [Server]
    public void ReturnStolenItem(ItemData itemData)
    {
        if (itemData == null) return;

        var itemConfig = ItemDatabase.GetItemById(itemData.configId);
        if (itemConfig != null)
        {
            PutInEmptySlot(itemConfig, itemData);
            RpcShowReturnedItemMessage(itemConfig.itemName);
        }
    }

    [ClientRpc]
    private void RpcShowReturnedItemMessage(string itemName)
    {
        // Показать сообщение игроку, что предмет возвращен
        Debug.Log($"Возвращен украденный предмет: {itemName}");
    }
    public void DropOnDie() {
        foreach (InventorySlot t in Slots) {
            if (t.ItemConfig == null) {
                continue;
            }
            DropItem(t.ItemData);
            t.ItemConfig = null;
            t.ItemData = null;
        }
    }
    
    // public bool HasItem(ItemConfig itemConfig, int amount, ItemRank minRank = ItemRank.C)
    // {
    //     if (itemConfig == null)
    //     {
    //         Debug.LogWarning("HasItem: itemConfig is null");
    //         return false;
    //     }
    //
    //     int count = 0;
    //     foreach (var slot in Slots)
    //     {
    //         if (slot.ItemConfig == itemConfig && (slot.ItemData?.rank ?? ItemRank.C) >= minRank)
    //         {
    //             count++;
    //             if (count >= amount) 
    //                 return true;
    //         }
    //     }
    //     return false;
    // }
    // [Command]
    public void CmdRemoveSpecificItems(ItemConfig itemConfig, int amount, ItemRank minRank)
    {
        int remaining = amount;
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemConfig == itemConfig && 
                Slots[i].ItemData.rank >= minRank)
            {
                if (Slots[i].SlotView != null)
                {
                    Slots[i].SlotView.ClearSlot();
                }
            
                Slots[i] = new InventorySlot();
                remaining--;
            
                if (remaining <= 0) break;
            }
        }
        RpcUpdateInventory();
    }
    public void CraftItem(ItemRecipe recipe)
    {
        if (!CanCraft(recipe))
        {
            Debug.LogWarning("Crafting conditions not met!");
            return;
        }

        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient != null && ingredient.itemConfig != null)
            {
                CmdRemoveSpecificItems(ingredient.itemConfig, ingredient.amount, ingredient.minRank);
            }
        }

        if (recipe.resultItem != null)
        {
            var itemData = new ItemData {
                configId = recipe.resultItem.name,
                rank = CalculateResultRank(recipe)
            };
            PutInEmptySlot(recipe.resultItem, itemData);
            Debug.Log($"Successfully crafted: {recipe.resultItem.itemName}");
        }
    }
    [Command]
    public void CmdCraftItem(string recipeId)
    {
        Debug.Log($"Server received craft request for: {recipeId}");
    
        var recipe = RecipeDatabase.GetRecipeById(recipeId);
        if (recipe == null)
        {
            Debug.LogError($"Recipe not found: {recipeId}");
            return;
        }

        // Дополнительная проверка на сервере
        if (!CanCraftLocally(recipe))
        {
            Debug.Log("Server validation failed - cannot craft");
            return;
        }

        // Удаление ингредиентов
        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient != null && ingredient.itemConfig != null)
            {
                Debug.Log($"Removing ingredient: {ingredient.itemConfig.name} x{ingredient.amount}");
                CmdRemoveItem(ingredient.itemConfig.name, ingredient.amount, ingredient.minRank);
            }
        }

        // Создание результата
        if (recipe.resultItem != null)
        {
            var result = new ItemData {
                configId = recipe.resultItem.name,
                rank = CalculateResultRank(recipe)
            };
            Debug.Log($"Creating result: {recipe.resultItem.itemName} x{recipe.resultAmount}");
            TargetAddCraftResult(connectionToClient, result.configId, result.rank);
        }
    }
    
    [TargetRpc]
    private void TargetAddCraftResult(NetworkConnection target, string itemId, ItemRank rank)
    {
        Debug.Log($"Received craft result: {itemId} (rank {rank})");
    
        var itemConfig = ItemDatabase.GetItemById(itemId);
        if (itemConfig != null)
        {
            var itemData = new ItemData {
                configId = itemId,
                rank = rank
            };
        
            if (PutInEmptySlot(itemConfig, itemData))
            {
                Debug.Log($"Successfully added to inventory: {itemConfig.itemName}");
            }
            else
            {
                Debug.LogWarning("No empty slots for crafted item!");
                // Здесь можно реализовать дроп предмета на землю
            }
        }
        else
        {
            Debug.LogError($"Item config not found for: {itemId}");
        }
    }
    private bool CanCraftLocally(ItemRecipe recipe)
    {
        foreach (var ingredient in recipe.ingredients)
        {
            if (!HasItem(ingredient.itemConfig, ingredient.amount, ingredient.minRank))
                return false;
        }
        return true;
    }
    public bool CanCraft(ItemRecipe recipe)
    {
        if (recipe == null) return false;
    
        foreach (var ingredient in recipe.ingredients)
        {
            if (ingredient == null || ingredient.itemConfig == null) 
                return false;
            
            bool hasEnough = false;
            int count = 0;
        
            foreach (var slot in Slots)
            {
                if (slot.ItemConfig == ingredient.itemConfig && 
                    slot.ItemData.rank >= ingredient.minRank)
                {
                    count++;
                    if (count >= ingredient.amount)
                    {
                        hasEnough = true;
                        break;
                    }
                }
            }
        
            if (!hasEnough) return false;
        }
    
        return true;
    }
    private ItemRank CalculateResultRank(ItemRecipe recipe)
    {
        return ItemRank.C;
    }
    
    [Command]
    public void CmdRemoveItem(string itemId, int amount, ItemRank minRank)
    {
        var itemConfig = ItemDatabase.GetItemById(itemId);
        if (itemConfig == null) return;

        int remaining = amount;
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].ItemConfig == itemConfig && 
                (Slots[i].ItemData?.rank ?? ItemRank.C) >= minRank)
            {
                Slots[i] = new InventorySlot();
                remaining--;
                if (remaining <= 0) break;
            }
        }
        RpcUpdateInventory();
    }
    [Command]
    public void CmdRemoveItemSimple(string itemId, int amount)
    {
        CmdRemoveItem(itemId, amount, ItemRank.C);
    }
    public bool HasItem(ItemConfig itemConfig, int amount, ItemRank minRank = ItemRank.C)
    {
        if (itemConfig == null)
        {
            Debug.LogWarning("HasItem: itemConfig is null");
            return false;
        }

        Debug.Log($"Checking for item: {itemConfig.itemName}, need {amount} with min rank {minRank}");
    
        int count = 0;
        foreach (var slot in Slots)
        {
            if (slot.ItemConfig == itemConfig)
            {
                var slotRank = slot.ItemData?.rank ?? ItemRank.C;
                Debug.Log($"Found item: {itemConfig.itemName}, rank {slotRank} in slot");
            
                if (slotRank >= minRank)
                {
                    count++;
                    if (count >= amount) 
                    {
                        Debug.Log($"Enough items found: {count} >= {amount}");
                        return true;
                    }
                }
            }
        }
    
        Debug.Log($"Not enough items: found {count}, need {amount}");
        return false;
    }
    
    public bool HasItem(string itemId, int amount, ItemRank minRank = ItemRank.C)
    {
        var itemConfig = ItemDatabase.GetItemById(itemId);
        if (itemConfig == null)
        {
            Debug.LogWarning($"Item not found: {itemId}");
            return false;
        }
        return HasItem(itemConfig, amount, minRank);
    }
    
    
    
    // [Command]
    // public void CmdRemoveSpecificItems(string itemId, int amount, ItemRank minRank)
    // {
    //     var itemConfig = ItemDatabase.GetItemById(itemId);
    //     if (itemConfig == null) return;
    //
    //     int remaining = amount;
    //     for (int i = 0; i < Slots.Count; i++)
    //     {
    //         if (Slots[i].ItemConfig == itemConfig && 
    //             Slots[i].ItemData.rank >= minRank)
    //         {
    //             if (Slots[i].SlotView != null)
    //             {
    //                 Slots[i].SlotView.ClearSlot();
    //             }
    //         
    //             Slots[i] = new InventorySlot();
    //             remaining--;
    //         
    //             if (remaining <= 0) break;
    //         }
    //     }
    //     RpcUpdateInventory();
    // }
    
    [ClientRpc]
    private void RpcUpdateInventory()
    {
        InventoryManager.Instance.InventoryView.SetData(this);
        OnInventoryChanged?.Invoke(); 
    }
    
    [Command]
    private void DropItem(ItemData itemData) {
        if (itemData == null) return;

        Vector3 dropPos = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
        ItemBase item = ItemFactory.Instance.CreateItemByData(itemData);
    
        if (item == null) {
            Debug.LogWarning($"Failed to drop item of type {itemData.Type}");
            return;
        }

        item.gameObject.SetActive(true);
        item.gameObject.transform.position = dropPos;
        NetworkServer.Spawn(item.gameObject);

        // Очищаем слот
        for (int i = 0; i < Slots.Count; i++) {
            if (Slots[i].ItemData == itemData) {
                if (Slots[i].SlotView != null) {
                    Slots[i].SlotView.ClearSlot();
                }
                Slots[i] = new InventorySlot();
                break;
            }
        }

        RpcUpdateInventory();
    }
    
}

[Serializable]
public class InventorySlot {
    public ItemConfig ItemConfig;
    public ItemData ItemData;
    [NonSerialized]
    public InventorySlotView SlotView;
}