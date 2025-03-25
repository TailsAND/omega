using UnityEngine;

public class OcclusionController : MonoBehaviour
{
    public Transform player; // Ссылка на персонажа
    public Camera mainCamera; // Ссылка на основную камеру
    public float alphaHidden = 0.5f; // Альфа-значение для скрытого состояния
    public float alphaVisible = 1f; // Альфа-значение для видимого состояния

    private SpriteRenderer playerRenderer;

    void Start()
    {
        playerRenderer = player.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Л Raycast для проверки, загораживает ли объект персонажа
        RaycastHit2D hit = Physics2D.Raycast(mainCamera.transform.position, player.position - mainCamera.transform.position);
        if (hit.collider != null && hit.collider.gameObject != player.gameObject)
        {
            // Если объект загораживает персонажа, то измените его видимость
            Color color = playerRenderer.color;
            color.a = alphaHidden; // Уменьшаем альфа-канал
            playerRenderer.color = color;
        }
        else
        {
            // Восстанавливаем видимость
            Color color = playerRenderer.color;
            color.a = alphaVisible; // Полная видимость
            playerRenderer.color = color;
        }
    }
}
