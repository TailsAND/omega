using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

//≈го теперь нужно подключить к префабу игрока, так же включить все панели инвентар€
//и поставить scale x = 0 y = 0
public class InventoryManager : NetworkBehaviour
{
    public GameObject inventorySlots;
    public GameObject inventoryQuests;
    public GameObject inventorySkills;
    private bool isOpened = false;
    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = gameObject.GetComponent<PlayerStats>();
        StartCoroutine(FindInventories());

    }

    // Update is called once per frame
    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }
        if (inventorySlots == null || inventorySkills == null || inventoryQuests == null)
        {
            inventorySlots = GameObject.Find("PanelInvent");
            inventorySkills = GameObject.Find("PanelProgres");
            inventoryQuests = GameObject.Find("PanelKvest");

        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            playerStats.UpdateUI();
            if (!isOpened)
            {
                InventoryOpen();
            }
            else
            {
                InventoryClose();
            }
        }

    }
    [Client]
    private void InventoryOpen()
    {

        inventorySlots.transform.localScale = Vector3.one;
        inventoryQuests.transform.localScale = Vector3.one;
        inventorySkills.transform.localScale = Vector3.one;
        isOpened = true;
    }
    [Client]
    private void InventoryClose()
    {
        inventorySlots.transform.localScale = Vector3.zero;
        inventoryQuests.transform.localScale = Vector3.zero;
        inventorySkills.transform.localScale = Vector3.zero;
        isOpened = false;
    }
    private IEnumerator FindInventories()
    {
        while (inventorySlots == null || inventorySkills == null || inventoryQuests == null)
        {
            inventorySlots = GameObject.Find("PanelInvent");
            inventorySkills = GameObject.Find("PanelProgres");
            inventoryQuests = GameObject.Find("PanelKvest");

            yield return null; // ∆дЄм один кадр, чтобы избежать зависани€

        }
    }

}
