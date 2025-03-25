using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI; // Не забудьте добавить этот using для работы с UI
public class MousePositionHandler : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
    public GameObject prefabToPlace; // Префаб, который будет устанавливаться (например, снег)
    public GameObject highlightPrefab; // Префаб для подсветки тайла
    private GameObject highlightInstance; // Экземпляр подсветки
    private Vector3Int currentCellPosition; // Текущая позиция тайла
    public Vector2Int offset; // Смещение для изменения тайлов
    private float lastChangeTime; // Время последнего изменения тайлов
    public float changeInterval = 0.1f; // Интервал между изменениями тайлов
    private bool isButtonActive = false; // Состояние кнопки
    void Start()
    {
        highlightInstance = Instantiate(highlightPrefab); // Создаем экземпляр подсветки
        highlightInstance.SetActive(false); // Скрываем подсветку по умолчанию
    }
    void Update()
    {
        if (!isButtonActive) return; // Если кнопка не активна, выходим из метода
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane));
        mouseWorldPosition.z = 0;
        Vector3Int cellPosition = tilemap.WorldToCell(mouseWorldPosition);
        if (tilemap.HasTile(cellPosition))
        {
            highlightInstance.transform.position = tilemap.GetCellCenterWorld(cellPosition);
            highlightInstance.SetActive(true);
            currentCellPosition = cellPosition;
        }
        else
        {
            highlightInstance.SetActive(false);
        }
        // Проверяем, зажата ли левая кнопка мыши
        if (Input.GetMouseButton(0) && highlightInstance.activeSelf)
        {
            // Проверяем, прошло ли достаточно времени с последнего изменения
            if (Time.time - lastChangeTime >= changeInterval)
            {
                ChangeTilesInArea(currentCellPosition, offset);
                lastChangeTime = Time.time; // Обновляем время последнего изменения
            }
        }
    }
    public void ToggleButton() // Метод для переключения состояния кнопки
    {
        isButtonActive = !isButtonActive; // Переключаем состояние
    }
    void ChangeTilesInArea(Vector3Int centerPosition, Vector2Int offset)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                // Применяем смещение к позиции
                Vector3Int position = new Vector3Int(centerPosition.x + x + offset.x, centerPosition.y + y + offset.y, centerPosition.z);
                // Проверяем, есть ли на позиции вода
                Collider2D hit = Physics2D.OverlapPoint(tilemap.GetCellCenterWorld(position));
                if (hit != null && hit.CompareTag("Water"))
                {
                    // Удаляем тайл, если он существует
                    if (tilemap.HasTile(position))
                    {
                        tilemap.SetTile(position, null); // Удаляем тайл
                    }
                    // Устанавливаем префаб на ту же позицию
                    Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                    Instantiate(prefabToPlace, worldPosition, Quaternion.identity); // Устанавливаем префаб
                }
            }
        }
    }
}
