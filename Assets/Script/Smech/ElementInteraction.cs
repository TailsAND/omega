using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ElementInteraction : MonoBehaviour
{
    public GameObject firePrefab; // Префаб огня
    public GameObject icePrefab; // Префаб льда
    public GameObject waterPrefab; // Префаб воды

    private GameObject currentWater; // Ссылка на текущий префаб воды

    void Update()
    {
        CheckElementInteractions();
    }

    private void CheckElementInteractions()
    {
        // Получаем все объекты в сцене
        GameObject[] fireObjects = GameObject.FindGameObjectsWithTag("Fire");
        GameObject[] iceObjects = GameObject.FindGameObjectsWithTag("Ice");

        foreach (GameObject fire in fireObjects)
        {
            foreach (GameObject ice in iceObjects)
            {
                // Проверяем расстояние между огнем и льдом
                if (Vector3.Distance(fire.transform.position, ice.transform.position) <= 0.5f)
                {
                    CreateWaterAtInteractionPoint(fire.transform.position, ice.transform.position);
                }
            }
        }
    }

    private void CreateWaterAtInteractionPoint(Vector3 firePosition, Vector3 icePosition)
    {
        // Если вода уже создана, выходим
        if (currentWater != null)
        {
            return;
        }

        // Находим среднюю позицию между огнем и льдом
        Vector3 waterPosition = (firePosition + icePosition) / 2;

        // Создаем префаб воды
        currentWater = Instantiate(waterPrefab, waterPosition, Quaternion.identity);

        // Удаляем огонь и лед по тегам
        GameObject fireObject = GameObject.FindGameObjectWithTag("Fire");
        GameObject iceObject = GameObject.FindGameObjectWithTag("Ice");

        if (fireObject != null)
        {
            Destroy(fireObject); // Удаляем объект огня
        }

        if (iceObject != null)
        {
            Destroy(iceObject); // Удаляем объект льда
        }
    }

    public void RemoveWater()
    {
        // Удаляем префаб воды, если он существует
        if (currentWater != null)
        {
            Destroy(currentWater);
            currentWater = null;
        }
    }
}
