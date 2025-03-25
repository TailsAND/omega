using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfo : MonoBehaviour
{ 
    public static ItemInfo instance;

    private Image BackGround;
    private Text Title;
    private Text Description;
    private Image Icon;
    private Button ExitButton;
    private Button UseButton;
    private Button DropButton;

    private Item InfoItem;
    private GameObject ItemObj;
    private InventorySlot CurrenSlot;

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


        ExitButton.onClick.AddListener(Close);
        UseButton.onClick.AddListener(Use);
        DropButton.onClick.AddListener(Drop);
    }
    public void ChangeInfo(Item item)
    { 
        Title.text = item.name;
        Description.text = item.Description;
        Icon.sprite = item.icon;

    }

    public void Use()
    { 
        UseOfItems.instance.Use(InfoItem);

        CurrenSlot.ClearSlot();
        Close();
    }

    public void Drop()
    {
        Vector3 DropPos = new Vector3(Player.instance.transform.position.x + 0.5f, Player.instance.transform.position.y, Player.instance.transform.position.z);
        ItemObj.SetActive(true);
        ItemObj.transform.position = DropPos;

        CurrenSlot.ClearSlot();
        Close();
    }

    public void Open(Item item, GameObject itemObj, InventorySlot currentSlot)
    { 
        ChangeInfo(item);
        InfoItem = item;
        ItemObj = itemObj;
        CurrenSlot = currentSlot;

        gameObject.transform.localScale = Vector3.one;
    }

    public void Close() 
    {
        gameObject.transform.localScale = Vector3.zero;
    }
}




















