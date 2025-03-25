using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject[] panels; // Массив для хранения панелей
    public Button[] buttons; // Массив для хранения кнопок
    private bool isOpened = false;
    private int lastOpened;
    void Start()
    {
        // Привязываем методы к кнопкам
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i; // Локальная переменная для правильного доступа в лямбда-выражении
            buttons[i].onClick.AddListener(() => ShowPanel(index));
        }

        // Деактивируем все панели, кроме первой
        //ShowPanel(0);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {

            if (!isOpened)
            {
                panels[lastOpened].transform.localScale = Vector3.one;
                isOpened = true;
            }
            else
            {
                panels[lastOpened].transform.localScale = Vector3.zero;
                isOpened = false;
            }
        }
    }
    // Метод для активации нужной панели
    public void ShowPanel(int panelIndex)
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i == panelIndex)
            {
                panels[i].transform.localScale = Vector3.one;
                lastOpened = i;
            }
            else
            {
                panels[i].transform.localScale = Vector3.zero;
            }
            //panels[i].SetActive(i == panelIndex); // Активируем нужную панель, остальные деактивируем
        }
    }
}