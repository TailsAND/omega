using Unity.VisualScripting;
using UnityEngine;

public class QuestGiver : MonoBehaviour
{
    public string questTitle;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            QuestManager questManager = FindObjectOfType<QuestManager>();
            questManager.CompleteQuest(questTitle);            
        }
    }
    

}