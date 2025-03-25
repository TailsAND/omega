using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseOfItems : MonoBehaviour
{
    public static UseOfItems instance;
    private PlayerStats playerStats; 

    private void Start()
    {
        instance = this;
        playerStats = FindObjectOfType<PlayerStats>(); 
    }

    public void Use(Item item)
    {
        if (item.isHealing)
        {
            Debug.Log("Вы восстановили здоровье на " + item.HealingPower);
            playerStats.UseItem(item); 
        }

        if (item.isMana)
        {
            Debug.Log("Вы восстановили ману на " + item.ManaPower);
            playerStats.UseItem(item); 
        }
    }
}
