using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using Mirror;

public class QuestManager : MonoBehaviour
{
    public List<Quest> quests = new();
    private Transform[] slots;

    public string[] DialogueTexts;
    public TMP_Text dialogueStart;
    public TMP_Text dialogueMiddle;
    public TMP_Text dialogueEnd;
    public GameObject start;
    public GameObject middle;
    public GameObject end;

    public PlayerStats playerStats;
    private DialogStart dialogStart;

    private Find Find;
    void Awake()
    {
        LoadAllQuests();
        PlayerPrefs.DeleteAll(); // На случай если надо всё удалить
    }
    void Start()
    {
        //LoadAllQuests();
        //playerStats = Find.FindUIElement<PlayerStats>("");
    }
    public void CompleteQuest(string title)
    {
        Quest quest = quests.Find(q => q.title == title);
        if (quest != null && !quest.isCompleted && quest.isStarted)
        {
            quest.CompleteQuest();
            playerStats.AddExperience(quest.experience);
        }
    }
    [Client]
    public List<Quest> GetActiveQuests()
    {
        return quests.FindAll(q => !q.isCompleted && q.isStarted);
    }

    public void FinishQuest()
    {

        Quest quest = quests.Find(q => q.title == dialogStart.title);
        
        if (isEnoughRequiredItemsToFinishQuest())
        {

            DeleteQuestItemsFromInventory();
            
            middle.SetActive(false);
            dialogueEnd.text = quest.thanks;
            end.SetActive(true);
            CompleteQuest(dialogStart.title);
            quest.SaveQuest();
        }
        else
        {
            dialogueMiddle.text = DialogueTexts[Random.Range(0, DialogueTexts.Length)];
        }
    }

    private bool isEnoughRequiredItemsToFinishQuest()
    {
        Quest quest = quests.Find(q => q.title == dialogStart.title);
        GameObject inventory = GameObject.Find("Slots_transform");
        
        List<Item> requiredItems = new();
        foreach (var item in quest.quest_items)
        {
            requiredItems.Add(item);
        }
        List<InventorySlot> slotsWithItems = new();
        
        CheckThroughPlayerInventoryForItems(slotsWithItems);


        bool allItemsAvailable = true;

        int count = 0;

        // Проходим по всем необходимым предметам
        foreach (var requiredItem in requiredItems)
        {
            List<InventorySlot> itemSlots = slotsWithItems.FindAll(slot => slot.SlotItem == requiredItem);


            if (itemSlots.Count < requiredItems.Count)
            {
                allItemsAvailable = false;
                Debug.Log($"Недостающий предмет: {requiredItem.name}"); 
            }
            else
            {
                count += itemSlots.Count; 
            }
        }

        return allItemsAvailable;
    }

    private void DeleteQuestItemsFromInventory()
    {
        Quest quest = quests.Find(q => q.title == dialogStart.title);
        
        List<Item> requiredItems = new();
        foreach (var item in quest.quest_items)
        {
            requiredItems.Add(item);
        }
        
        List<InventorySlot> slotsWithItems = new();
        int needToDelete = requiredItems.Count;
        int deleted = 0;

        CheckThroughPlayerInventoryForItems(slotsWithItems);
        
        // Удаляем все необходимые предметы из инвентаря
        foreach (var requiredItem in requiredItems)
        {
            foreach (var slot in slotsWithItems)
            {
                if (slot.SlotItem == requiredItem && deleted < needToDelete)
                {
                    slot.ClearSlot(); // Удаляем предмет для квеста
                    deleted++;
                }
                else if (deleted >= needToDelete)
                {
                    break;
                }
            }
        }
    }

    private List<InventorySlot> CheckThroughPlayerInventoryForItems(List<InventorySlot> slotsWithItems)
    {
        GameObject inventory = GameObject.Find("Slots_transform");

        // Проходим по всем слотам в инвентаре
        for (int i = 0; i < inventory.transform.childCount; i++)
        {
            Transform child = inventory.transform.GetChild(i);
            InventorySlot slot = child.GetComponent<InventorySlot>();

            if (slot != null && slot.SlotItem != null) 
            {
                slotsWithItems.Add(slot); 
            }
        }

        return slotsWithItems;
    }
    private List<Quest> LoadQuests(string title = null)
    {
        List<Quest> loadedQuests = new List<Quest>();

        if (string.IsNullOrEmpty(title))
        {
            // Загружаем все квесты
            for (int i = 0; i < PlayerPrefs.GetInt("questCount", 0); i++)
            {
                string json = PlayerPrefs.GetString("quest_" + i);
                if (!string.IsNullOrEmpty(json))
                {
                    Quest quest = JsonUtility.FromJson<Quest>(json);
                    loadedQuests.Add(quest);
                }
            }
        }
        else
        {
            // Загружаем конкретный квест по названию
            string json = PlayerPrefs.GetString("quest_" + title);
            if (!string.IsNullOrEmpty(json))
            {
                Quest quest = JsonUtility.FromJson<Quest>(json);
                loadedQuests.Add(quest);
            }
        }

        return loadedQuests;
    }
    [Client]
    public void LoadAllQuests()
    {
        foreach (var quest in quests)
        {
            // Загружаем квесты по названию
            List<Quest> loadedQuests = LoadQuests(quest.title);

            // Проверяем, если список загруженных квестов не пустой
            if (loadedQuests.Count > 0)
            {
                for (int i = 0; i < loadedQuests.Count; i++)
                {
                    Quest loadedQuest = loadedQuests[i]; // Берем первый загруженный квест

                    // Обновите квест в списке текущих квестов
                    quest.isStarted = loadedQuest.isStarted;
                    quest.isCompleted = loadedQuest.isCompleted;
                }
            }
        }
    }
    /// <summary>
    /// Позволяет вызывать тот квест, к которому подошёл игрок
    /// </summary>
    /// <param name="dialog"></param>
    public void SetQuest(DialogStart dialog)
    {
        if (dialog != null)
        {
            dialogStart = dialog;
            Debug.Log(dialog.title);
        }
    }
    public void StartQuest()
    {
        Quest quest = quests.Find(q => q.title == dialogStart.title);
        quest.isStarted = true;
        start.transform.localScale = Vector3.zero;
        quest.SaveQuest();
    }
    public void Close()
    {
        start.transform.localScale = Vector3.zero;
        middle.transform.localScale = Vector3.zero;
        end.transform.localScale = Vector3.zero;
    }
    [Client]
    private void FindAll()
    {
        dialogueStart = Find.FindUIElement<TMP_Text>("DialogStart");
        dialogueMiddle = Find.FindUIElement<TMP_Text>("DialogMiddle");
        dialogueEnd = Find.FindUIElement<TMP_Text>("DialogFinish");
        start = Find.FindGameObject("PanelDialogStart");
        middle = Find.FindGameObject("PanelDialogMiddle");
        end = Find.FindGameObject("PanelDialogFinish");
    }
}
