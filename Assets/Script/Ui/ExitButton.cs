using UnityEngine;
using UnityEngine.SceneManagement; // Для работы со сценами
using UnityEngine.UI; // Для работы с UI (если нужно)

public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
#if UNITY_EDITOR
            // Если игра запущена в редакторе, останавливаем воспроизведение
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // Если игра запущена не в редакторе, выходим из приложения
        Application.Quit();
#endif
    }
}
