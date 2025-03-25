using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.IO;

[System.Serializable]
public class Quest
{
    [Header("NPC_Text")]
    public string title;
    public string description;
    public string question;
    public string thanks;
    [Header("Rewards")]
    public int experience;
    public List<Item> reward_items;
    public List<Item> quest_items;

    public bool isCompleted;
    public bool isStarted;

    //public Quest(string title, string description, string question, string thanks, int experience, List<Item> items, List<Item> quest_items)
    //{
    //    this.title = title;
    //    this.description = description;
    //    this.question = question;
    //    this.thanks = thanks;
    //    this.experience = experience;

    //    this.reward_items = items;
    //    this.quest_items = quest_items;

    //    this.isCompleted = false;
    //    this.isStarted = false;
    //}


    public void CompleteQuest()
    {
        isCompleted = true;
        foreach (var item in reward_items)
        {
            if (item != null)
            {
                Inventory.instance.PutInEmptySlot(item, null);
            }
        }
    }
    public void SaveQuest()
    {
        string json = JsonUtility.ToJson(this);
        PlayerPrefs.SetString("quest_" + this.title, json);
        PlayerPrefs.Save();
    }
}