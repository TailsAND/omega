using UnityEngine;
using UnityEngine.UI;

public class PanelManagerMenu : MonoBehaviour
{
    public GameObject[] panels;  // Массив всех панелей
    public Button[] buttons;    // Массив всех кнопок (опционально, если нужно менять цвет и т.д.)

    private int currentPanelIndex = -1;  // Индекс текущей открытой панели (-1 = ничего не открыто)

    void Start()
    {
        // Закрываем все панели при старте
        CloseAllPanels();

        // Назначаем обработчики нажатий для всех кнопок
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;  // Важно: сохраняем индекс в локальной переменной для замыкания
            buttons[i].onClick.AddListener(() => TogglePanel(index));
        }
    }

    public void TogglePanel(int panelIndex)
    {
        // Если нажали на уже открытую панель — закрываем её
        if (currentPanelIndex == panelIndex)
        {
            panels[panelIndex].SetActive(false);
            currentPanelIndex = -1;
        }
        else
        {
            // Закрываем все панели
            CloseAllPanels();

            // Открываем нужную
            panels[panelIndex].SetActive(true);
            currentPanelIndex = panelIndex;
        }
    }

    private void CloseAllPanels()
    {
        foreach (GameObject panel in panels)
        {
            panel.SetActive(false);
        }
    }
}