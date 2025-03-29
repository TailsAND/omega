using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuestUI : MonoBehaviour
{
    public TMP_Text questListText;
    private QuestManager questManager;
    private Find find;
    public void UpdateQuestList()
    {
        questListText.text = "Задания:\n";

        if (questManager == null)
        {
            //Костыль
            questManager = GameObject.Find("QuestManager").GetComponent<QuestManager>();
        }
        else;

        foreach (var quest in questManager.GetActiveQuests())
        {
            questListText.text += "- " + quest.title + ": " + quest.description + "\n";
        }
    }
}