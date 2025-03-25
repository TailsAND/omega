using UnityEngine;
using TMPro; // Для работы с TextMeshPro
using UnityEngine.EventSystems;
using System.Collections; // Для работы с событиями

public class ButtonColorChange : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI buttonText; // Ссылка на текст кнопки
    private Color originalColor; // Исходный цвет текста
    private Color targetColor; // Цвет, к которому мы стремимся
    private float transitionDuration = 0.5f; // Длительность перехода в секундах

    void Start()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        originalColor = new Color(0.5f, 0.5f, 0.5f); // Серый цвет
        buttonText.color = originalColor; // Устанавливаем исходный цвет текста
        targetColor = originalColor; // Изначально целевой цвет такой же
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetColor = Color.white; // Цвет текста при наведении
        StopAllCoroutines(); // Останавливаем все корутины перед началом новой
        StartCoroutine(ChangeColor(originalColor, targetColor));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetColor = originalColor; // Возвращаемся к исходному цвету
        StopAllCoroutines(); // Останавливаем все корутины перед началом новой
        StartCoroutine(ChangeColor(buttonText.color, targetColor));
    }

    private IEnumerator ChangeColor(Color startColor, Color endColor)
    {
        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            buttonText.color = Color.Lerp(startColor, endColor, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime; // Увеличиваем время
            yield return null; // Ждем следующего кадра
        }

        buttonText.color = endColor; // Устанавливаем конечный цвет
    }
}
