using UnityEngine;

public class InventoryView : MonoBehaviour {
    [SerializeField]
    private InventorySlotView[] inventorySlots = new InventorySlotView[24];

    public void SetData(PlayerInventory playerInventory) {
        for (int index = 0; index < playerInventory.Slots.Count; index++) {
            InventorySlot slot = playerInventory.Slots[index];
            if (slot != null && slot.ItemConfig != null) {
                // Проверяем, не надет ли уже этот предмет
                bool isEquipped = PlayerEquipment.Instance.GetAllItems().ContainsKey(slot.ItemConfig.itemType) && 
                                  PlayerEquipment.Instance.GetItemConfig(slot.ItemConfig.itemType) == slot.ItemConfig;
        
                if (!isEquipped) {
                    inventorySlots[index].PutInSlot(slot.ItemConfig, slot.ItemData);
                } else {
                    inventorySlots[index].ClearSlot();
                }
            } else {
                inventorySlots[index].ClearSlot();
            }
        }
    }


    public void PutInEmptySlot(ItemConfig itemConfig, ItemData itemData) {
        for (int i = 0; i < inventorySlots.Length; i++) {
            if (inventorySlots[i].slotItemConfig == null) {
                inventorySlots[i].PutInSlot(itemConfig,itemData);
                return;
            }
        }
    }
}