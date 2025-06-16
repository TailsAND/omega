using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel; // Панель диалога
    public TextMeshProUGUI dialogText; // Текст диалога (TMP)
    public AudioSource audioSource; // Аудиоисточник для голоса NPC
    public GameObject interactHint; // Подсказка "Нажми E"

    [Header("Dialog Settings")]
    public Dialog[] dialogs; // Массив реплик NPC
    private int currentDialogIndex = 0;
    private bool isDialogActive = false;
    private bool isPlayerInTrigger = false;
    private Collider2D npcCollider;
    private GameObject currentPlayer; // Текущий игрок в триггере

    private void Start()
    {
        panel.SetActive(false);
        interactHint.SetActive(false);
        npcCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsLocalPlayer(other))
        {
            isPlayerInTrigger = true;
            currentPlayer = other.gameObject;
            interactHint.SetActive(true); // Показываем подсказку
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == currentPlayer)
        {
            isPlayerInTrigger = false;
            interactHint.SetActive(false); // Скрываем подсказку
            
            if (isDialogActive)
            {
                ForceEndDialog(); // Закрываем диалог, если игрок ушёл
            }
        }
    }

    private void Update()
    {
        // Если игрок в триггере и нажал E
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.E))
        {
            if (!isDialogActive)
            {
                StartDialog(); // Начинаем диалог
            }
            else
            {
                ShowNextDialog(); // Продолжаем диалог
            }
        }
    }

    // Проверка, является ли игрок локальным (для Mirror)
    private bool IsLocalPlayer(Collider2D playerCollider)
    {
        NetworkIdentity networkIdentity = playerCollider.GetComponent<NetworkIdentity>();
        return networkIdentity != null && networkIdentity.isLocalPlayer;
    }

    private void StartDialog()
    {
        isDialogActive = true;
        panel.SetActive(true);
        interactHint.SetActive(false); // Скрываем подсказку
        ShowNextDialog();
        Debug.Log("Диалог начат (по нажатию E)");
    }

    private void ShowNextDialog()
    {
        if (currentDialogIndex < dialogs.Length)
        {
            dialogText.text = dialogs[currentDialogIndex].text;
            
            if (dialogs[currentDialogIndex].voiceClip != null)
            {
                audioSource.Stop();
                audioSource.clip = dialogs[currentDialogIndex].voiceClip;
                audioSource.Play();
            }
            
            currentDialogIndex++;
        }
        else
        {
            EndDialog();
        }
    }

    private void EndDialog()
    {
        panel.SetActive(false);
        currentDialogIndex = 0;
        isDialogActive = false;
        interactHint.SetActive(true); // Снова показываем подсказку
        Debug.Log("Диалог завершен (нормально)");
    }

    // Принудительное завершение диалога
    private void ForceEndDialog()
    {
        panel.SetActive(false);
        currentDialogIndex = 0;
        isDialogActive = false;
        audioSource.Stop();
        Debug.Log("Диалог прерван (игрок вышел из зоны)");
    }
}

[System.Serializable]
public class Dialog
{
    public string text; // Текст реплики
    public AudioClip voiceClip; // Озвучка реплики
}