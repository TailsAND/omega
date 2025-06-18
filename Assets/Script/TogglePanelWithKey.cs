using UnityEngine;

public class TogglePanelWithKey : MonoBehaviour
{
    public GameObject panel; // Ссылка на вашу UI панель
    public KeyCode toggleKey = KeyCode.P; // Клавиша для переключения (по умолчанию P)

    void Update()
    {
        // Проверяем нажатие клавиши
        if (Input.GetKeyDown(toggleKey))
        {
            // Переключаем активное состояние панели
            if (panel != null)
            {
                panel.SetActive(!panel.activeSelf);
            }
        }
    }
}