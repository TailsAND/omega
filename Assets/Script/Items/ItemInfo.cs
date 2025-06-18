using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{ 
    public static ItemInfo instance;
    public ItemData itemData;
    private Image BackGround;
    private Text Title;
    private Text Description;
    private Image Icon;
    private Button ExitButton;
    private Button UseButton;
    private Button EquipButton;
    private Button DropButton;
    private ItemConfig _infoItemConfig;
    private InventorySlotView CurrenSlot;
    
    private void Start()
    { 
        instance = this;

        BackGround = GetComponent<Image>();
        Title = transform.GetChild(0).GetComponent<Text>();
        Description = transform.GetChild(1) .GetComponent<Text>();
        Icon = transform.GetChild(2).GetComponent<Image>();
        ExitButton = transform.GetChild(3).GetComponent<Button>();
        UseButton = transform.GetChild(4).GetComponent<Button>();
        DropButton = transform.GetChild(5).GetComponent<Button>();
        EquipButton = transform.GetChild(6).GetComponent<Button>();


        ExitButton.onClick.AddListener(Close);
        UseButton.onClick.AddListener(Use);
        DropButton.onClick.AddListener(Drop);
        EquipButton.onClick.AddListener(Equip);
    }
    public void ChangeInfo(ItemConfig itemConfig)
    { 
        Title.text = itemConfig.name;
        Description.text = itemConfig.Description;
        Icon.sprite = itemConfig.icon;

    }

    public void Use() { 
        UseOfItems.instance.Use(_infoItemConfig);
    
        // Удаляем предмет из инвентаря
        var inventory = InventoryManager.Instance.PlayerSkillController.GetComponent<PlayerInventory>();
        for (int i = 0; i < inventory.Slots.Count; i++) {
            if (inventory.Slots[i].ItemConfig == _infoItemConfig && 
                inventory.Slots[i].ItemData == itemData) {
                inventory.Slots[i] = new InventorySlot();
                break;
            }
        }
    
        Close();
        CurrenSlot.ClearSlot();
    
        // Обновляем инвентарь
        InventoryManager.Instance.InventoryView.SetData(inventory);
    }

    public void Drop()
    {
        Vector3 DropPos = new Vector3(Player.instance.transform.position.x + 0.5f, Player.instance.transform.position.y, Player.instance.transform.position.z);
        
        ItemBase item = ItemFactory.Instance.CreateItemByData(itemData);
        
        item.gameObject.SetActive(true);
        item.gameObject.transform.position = DropPos;
        Close();
        CurrenSlot.ClearSlot();
    }

    public void Equip() {
        var equipment = CurrenSlot.slotItemConfig;
        if (equipment is not ItemConfig config) { return; }
            PlayerEquipment.Instance.WearItem(config,CurrenSlot.slotItemData);
            CurrenSlot.ClearSlot();
            Close();
    }

    public void Open(ItemConfig itemConfig,ItemData itemData ,InventorySlotView currentSlot)
    { 
        ChangeInfo(itemConfig);
        _infoItemConfig = itemConfig;
        CurrenSlot = currentSlot;
        this.itemData = itemData;
        gameObject.transform.localScale = Vector3.one;
        UseButton.gameObject.SetActive(itemData.IsUsable);
        EquipButton.gameObject.SetActive(!itemData.IsUsable);
    }

    public void Close() 
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}




















