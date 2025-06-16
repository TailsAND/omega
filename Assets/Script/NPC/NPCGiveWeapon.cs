using System;
using Unity;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class NPCGiveWeapon : MonoBehaviour{
    [SerializeField] private ItemData itemData;
    [SerializeField] private ItemConfig itemConfig;
    private bool _first_meet = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if (!_first_meet) {
            InventoryManager.Instance.PlayerSkillController.gameObject.GetComponent<PlayerInventory>()
            .PutInEmptySlot(itemConfig, itemData);
            _first_meet = true;
        }
    }
}
