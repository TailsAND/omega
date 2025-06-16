using UnityEngine;
using UnityEngine.UI;

public class EscapePanelController : MonoBehaviour
{
    public GameObject escapePanel; 
    public GameObject[] otherUI; // Теперь это массив UI-элементов

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Проверяем, что ни один из otherUI не активен
            bool canOpenEscapePanel = true;
            foreach (var uiElement in otherUI)
            {
                if (uiElement != null && uiElement.activeInHierarchy)
                {
                    canOpenEscapePanel = false;
                    break;
                }
            }

            if (canOpenEscapePanel)
            {
                ToggleEscapePanel();
            }
        }
    }

    private void ToggleEscapePanel()
    {
        bool newState = !escapePanel.activeSelf;
        escapePanel.SetActive(newState);
        
        if (newState)
        {
            Debug.Log("Escape panel opened");
            // Дополнительные действия при открытии
        }
        else
        {
            Debug.Log("Escape panel closed");
            // Дополнительные действия при закрытии
        }
    }

    public void OnContinueButton()
    {
        escapePanel.SetActive(false);
    }
    
    public void OnQuitButton()
    {
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}