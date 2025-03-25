using System.Collections;
using UnityEngine;

public class LightIntensityController : MonoBehaviour
{
    public Light lightSource; // Ссылка на источник света
    public float targetIntensity = 0.30f; // Целевая интенсивность
    public float returnIntensity = 0.80f; // Интенсивность, на которую нужно вернуться
    public float durationToReduce = 10f; // Время для понижения интенсивности
    public float delayBeforeReturn = 30f; // Время ожидания перед возвратом

    void Start()
    {
        // Запускаем корутину для управления интенсивностью света
        StartCoroutine(AdjustLightIntensity());
    }

    private IEnumerator AdjustLightIntensity()
    {
        // Понижаем интенсивность света
        float startIntensity = lightSource.intensity;
        float elapsedTime = 0f;

        while (elapsedTime < durationToReduce)
        {
            lightSource.intensity = Mathf.Lerp(startIntensity, targetIntensity, elapsedTime / durationToReduce);
            elapsedTime += Time.deltaTime;
            yield return null; // Ждем до следующего кадра
        }

        // Устанавливаем окончательную интенсивность
        lightSource.intensity = targetIntensity;

        // Ждем 30 секунд
        yield return new WaitForSeconds(delayBeforeReturn);

        // Возвращаем интенсивность света
        elapsedTime = 0f;
        while (elapsedTime < durationToReduce)
        {
            lightSource.intensity = Mathf.Lerp(targetIntensity, returnIntensity, elapsedTime / durationToReduce);
            elapsedTime += Time.deltaTime;
            yield return null; // Ждем до следующего кадра
        }

        // Устанавливаем окончательную интенсивность
        lightSource.intensity = returnIntensity;
    }
}
