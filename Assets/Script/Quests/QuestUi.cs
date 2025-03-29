using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class QuestUI : MonoBehaviour
{
    public TMP_Text questListText;
    public QuestManager questManager;
    private Find find;

    void Start()
    {
        questManager = find.FindUIElement<QuestManager>("QuestManager");
        UpdateQuestList();
    }

    public void UpdateQuestList()
    {
        questListText.text = "Задания:\n";
        foreach (Quest quest in questManager.GetActiveQuests())
        {
            questListText.text += "- " + quest.title + ": " + quest.description + "\n";
        }
    }
}