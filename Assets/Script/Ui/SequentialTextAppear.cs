using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SequentialTextAppear : MonoBehaviour
{
    public TMP_Text[] texts; // Массив из 3 текстовых элементов
    public Button button; // Кнопка, которая появится в конце
    public float fadeDuration = 1f; // Длительность появления каждого элемента
    public float delayBetweenTexts = 0.5f; // Задержка между появлениями текстов
    public float delayBeforeButton = 1f; // Задержка перед появлением кнопки

    void OnEnable()
    {
        // Сначала делаем все тексты и кнопку невидимыми
        foreach (var text in texts)
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            text.gameObject.SetActive(false);
        }

        if (button != null)
        {
            // Если у кнопки есть TMP_Text
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.color = new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0);
            }
            
            // Делаем всю кнопку прозрачной
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(buttonImage.color.r, buttonImage.color.g, buttonImage.color.b, 0);
            }
            
            button.gameObject.SetActive(false);
            button.interactable = false;
        }

        // Запускаем корутину появления
        StartCoroutine(ShowElementsSequentially());
    }

    IEnumerator ShowElementsSequentially()
    {
        // Показываем тексты один за другим
        foreach (var text in texts)
        {
            text.gameObject.SetActive(true);
            yield return StartCoroutine(FadeElement(text, 0, 1, fadeDuration));
            yield return new WaitForSeconds(delayBetweenTexts);
        }

        // Задержка перед показом кнопки
        yield return new WaitForSeconds(delayBeforeButton);

        // Показываем кнопку, если она есть
        if (button != null)
        {
            button.gameObject.SetActive(true);
            
            // Плавно появляется изображение кнопки
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                yield return StartCoroutine(FadeImage(buttonImage, 0, 1, fadeDuration));
            }
            
            // Плавно появляется текст кнопки
            TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                yield return StartCoroutine(FadeElement(buttonText, 0, 1, fadeDuration));
            }
            
            button.interactable = true;
        }
    }

    IEnumerator FadeElement(TMP_Text element, float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        Color startColor = element.color;
        startColor.a = startAlpha;
        element.color = startColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            Color newColor = element.color;
            newColor.a = alpha;
            element.color = newColor;
            yield return null;
        }

        Color endColor = element.color;
        endColor.a = endAlpha;
        element.color = endColor;
    }

    IEnumerator FadeImage(Image image, float startAlpha, float endAlpha, float duration)
    {
        float time = 0;
        Color startColor = image.color;
        startColor.a = startAlpha;
        image.color = startColor;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, time / duration);
            Color newColor = image.color;
            newColor.a = alpha;
            image.color = newColor;
            yield return null;
        }

        Color endColor = image.color;
        endColor.a = endAlpha;
        image.color = endColor;
    }
}