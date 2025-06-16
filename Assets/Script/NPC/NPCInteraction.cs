using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class NPCInteraction : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject panel;
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI playerResponseText;
    public GameObject responseButton;
    public AudioSource audioSource; // Аудиоисточник в панели
    public GameObject interactHint;

    [Header("Dialog Settings")]
    public Dialog[] dialogs;
    private int currentDialogIndex = 0;
    private bool isDialogActive = false;
    private bool isPlayerInTrigger = false;
    private GameObject currentPlayer;

    [System.Serializable]
    public class Dialog
    {
        public string npcText;
        public AudioClip npcVoice;
        public string playerResponse;
        public AudioClip playerVoice;
    }

    private void Start()
    {
        ResetDialogState();
    }

    private void ResetDialogState()
    {
        panel.SetActive(false);
        interactHint.SetActive(false);
        responseButton.SetActive(false);
        currentDialogIndex = 0;
        
        // Очищаем аудиодорожку
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null; // Важно: сбрасываем клип
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.E) && !isDialogActive)
        {
            StartDialog();
        }
    }

    private void StartDialog()
    {
        currentDialogIndex = 0;
        isDialogActive = true;
        panel.SetActive(true);
        interactHint.SetActive(false);
        ShowNPCDialog();
    }

    private void ShowNPCDialog()
    {
        if (currentDialogIndex < dialogs.Length)
        {
            dialogText.text = dialogs[currentDialogIndex].npcText;
            
            if (audioSource != null && dialogs[currentDialogIndex].npcVoice != null)
            {
                audioSource.Stop();
                audioSource.clip = dialogs[currentDialogIndex].npcVoice;
                audioSource.Play();
            }

            playerResponseText.text = dialogs[currentDialogIndex].playerResponse;
            responseButton.SetActive(true);
        }
        else
        {
            EndDialog();
        }
    }

    public void OnResponseButtonClick()
    {
        if (currentDialogIndex < dialogs.Length)
        {
            if (audioSource != null && dialogs[currentDialogIndex].playerVoice != null)
            {
                audioSource.Stop();
                audioSource.clip = dialogs[currentDialogIndex].playerVoice;
                audioSource.Play();
            }

            responseButton.SetActive(false);
            currentDialogIndex++;
            ShowNPCDialog();
        }
    }

    private void EndDialog()
    {
        ResetDialogState();
        isDialogActive = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsLocalPlayer(other))
        {
            isPlayerInTrigger = true;
            currentPlayer = other.gameObject;
            interactHint.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.gameObject == currentPlayer)
        {
            isPlayerInTrigger = false;
            interactHint.SetActive(false);
            if (isDialogActive) EndDialog();
        }
    }

    private bool IsLocalPlayer(Collider2D playerCollider)
    {
        NetworkIdentity networkIdentity = playerCollider.GetComponent<NetworkIdentity>();
        return networkIdentity != null && networkIdentity.isLocalPlayer;
    }
}