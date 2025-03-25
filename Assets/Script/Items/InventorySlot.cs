using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Item SlotItem;
    public GameObject ItemObj;

    Image icon;
    Button button;

    private void Start()
    {
        icon = gameObject.transform.GetChild(0).GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(ShowInfo);
    }
    public void PutInSlot(Item item, GameObject obj)
    {
        icon.sprite = item.icon;
        SlotItem = item;
        ItemObj = obj;
        icon.enabled = true;
    }

    void ShowInfo()
    {
        if (SlotItem != null) 
        ItemInfo.instance.Open(SlotItem, ItemObj, this);
    }
    public void ClearSlot()
    { 
        SlotItem = null;
        ItemObj = null;
        icon.sprite = null;
        icon.enabled = false;
    }

}
 
       

