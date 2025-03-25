using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class NPCDialogue : MonoBehaviour
{
    [Header("What npc says")]
    public string[] text;
    [Header("UI")]
    public TMP_Text DialogueText;
    public GameObject panel;
    public GameObject DialogueButton;

    private bool isPlayerNearby = false;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Talk();
        }
    }
    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerNearby = true;
            DialogueButton.SetActive(true);
          
        }
        else
        {
            DialogueButton.SetActive(false);
        }
    }
    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            isPlayerNearby = false;
            DialogueButton.SetActive(false);
        }
    }
    public void Talk()
    {
        DialogueText.text = text[Random.Range(0, text.Length)];
        panel.SetActive(true);
    }

}
