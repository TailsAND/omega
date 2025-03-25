using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Perehod_Global_on_local : MonoBehaviour
{
  // Имя сцены, на которую вы хотите переключиться
    public string sceneName;

    // Метод для переключения на сцену
    public void SwitchScene()
    {
        // Проверка на пустое имя сцены
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("Имя сцены не задано!");
        }
    }

    // Для примера, можно привязать этот метод к кнопке
    public void OnClick()
    {
        SwitchScene();
    }
}
