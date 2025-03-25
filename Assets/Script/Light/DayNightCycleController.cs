using System.Collections;
using UnityEngine;

public class DayNightCycleController : MonoBehaviour
{
    public Light lightSource; // Ссылка на компонент Light
    public GameObject fireObject; // Ссылка на объект огня
    public float dayDuration = 180f; // Длительность дня в секундах
    public float nightDuration = 80f; // Длительность ночи в секундах

    private void Start()
    {
        if (lightSource == null)
        {
            lightSource = GetComponent<Light>(); // Получаем компонент Light, если он не назначен
        }

        StartCoroutine(DayNightCycle());
    }

    private IEnumerator DayNightCycle()
    {
        while (true) // Бесконечный цикл
        {
            // Дневной цикл
            lightSource.enabled = false; // Отключаем свет
            if (fireObject != null)
            {
                fireObject.SetActive(false); // Деактивируем объект огня
            }

            yield return new WaitForSeconds(dayDuration); // Ждем длительность дня

            // Ночной цикл
            lightSource.enabled = true; // Включаем свет (если нужно)
            if (fireObject != null)
            {
                fireObject.SetActive(true); // Активируем объект огня
            }

            yield return new WaitForSeconds(nightDuration); // Ждем длительность ночи
        }
    }
}
