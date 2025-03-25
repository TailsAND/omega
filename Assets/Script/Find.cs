using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Find
{
    /// <summary>
    /// Гарантированно находить компонент игрового объекта, если он уже существует и он активный
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
    public T FindUIElement<T>(string name) where T : Component
    {
        T foundElement = null;
        float timeout = 1f; // Максимальное время ожидания в секундах
        float checkInterval = 0.1f; // Интервал проверки в секундах
        float elapsedTime = 0f;

        while (foundElement == null && elapsedTime < timeout)
        {
            foundElement = GameObject.Find(name)?.GetComponent<T>();
            elapsedTime += checkInterval;
            System.Threading.Thread.Sleep((int)(checkInterval * 100)); // Подождать перед следующей проверкой
        }
        return foundElement;
    }

    /// <summary>
    /// Ищет игровые объекты, не компоненты, где name это название активного объекта
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject FindGameObject(string name)
    {
        GameObject foundObject = null;
        float timeout = 1f; // Максимальное время ожидания в секундах
        float checkInterval = 0.1f; // Интервал проверки в секундах
        float elapsedTime = 0f;

        while (foundObject == null && elapsedTime < timeout)
        {
            foundObject = GameObject.Find(name);
            elapsedTime += checkInterval;
            System.Threading.Thread.Sleep((int)(checkInterval * 100)); // Подождать перед следующей проверкой
        }
        return foundObject;
    }
}
