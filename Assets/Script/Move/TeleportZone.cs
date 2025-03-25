using UnityEngine;
using UnityEngine.UI;

public class TeleportZone : MonoBehaviour
{
    public GameObject confirmationPanel; // Панель подтверждения
    public GameObject targetObject; // Объект, на позицию которого будет телепортирован игрок
    private GameObject player; // Ссылка на игрока

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Убедитесь, что тег игрока установлен в "Player"
        {
            player = other.gameObject; // Сохраняем ссылку на игрока
            // Показываем панель подтверждения
            confirmationPanel.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Убедитесь, что тег игрока установлен в "Player"
        {
            // Скрываем панель подтверждения, если игрок покинул зону
            confirmationPanel.SetActive(false);
            player = null; // Сбрасываем ссылку на игрока
        }
    }

    // Метод для подтверждения перемещения
    public void ConfirmTeleport()
    {
        if (player != null && targetObject != null)
        {
            player.transform.position = targetObject.transform.position; // Перемещаем игрока на позицию целевого объекта
        }
        confirmationPanel.SetActive(false); // Скрываем панель
    }

    // Метод для отмены перемещения
    public void CancelTeleport()
    {
        confirmationPanel.SetActive(false); // Скрываем панель
    }
}
