using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Mirror;

public class DialogStart : NetworkBehaviour
{
    public string title;
    public GameObject Button;

    private PlayerStats player;
    private bool isPlayerNearby = false;
    private Quest quest;
    private QuestManager questManager;
    private void Start()
    {
        player = FindObjectOfType<PlayerStats>();
        questManager = FindObjectOfType<QuestManager>();
        quest = questManager.quests.Find(q => q.title == title);
    }
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && !quest.isCompleted)
        {

            if (!quest.isStarted)
            {
                questManager.start.transform.localScale = Vector3.one;
                questManager.dialogueStart.text = quest.description;
            }
            else if (quest.isStarted)
            {
                questManager.middle.transform.localScale = Vector3.one;
                questManager.dialogueMiddle.text = quest.question;
            }

        }
    }
    [Client]
    void OnTriggerEnter2D(Collider2D collider)
    {
        questManager.playerStats = collider.GetComponent<PlayerStats>();
        player = collider.GetComponent<PlayerStats>();
        questManager.LoadAllQuests();
        QuestManager questmanager = FindObjectOfType<QuestManager>();
        Quest quest = questmanager.quests.Find(q => q.title == title);

        if (collider.CompareTag("Player") && !quest.isCompleted)
        {
            isPlayerNearby = true;
            Button.SetActive(true);
            questmanager.SetQuest(GetComponent<DialogStart>());
        }
        else
        {
            Button.SetActive(false);
            Debug.Log("merged");

        }
    }
    [Client]
    void OnTriggerExit2D(Collider2D collider)
    {

        quest = questManager.quests.Find(q => q.title == title);
        if (collider.CompareTag("Player") && !quest.isCompleted)
        {
            isPlayerNearby = false;
            Button.SetActive(false);
            questManager.SetQuest(GetComponent<DialogStart>());
        }
        else
        {
            Button.SetActive(false);

        }

    }
}
