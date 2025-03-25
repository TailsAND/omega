using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SoilChanger : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
    public KeyCode attackKey; // Клавиша атаки
    public GameObject prefabToPlace; // Префаб, который будет устанавливаться (например, земля)
    public GameObject highlightPrefab; // Префаб для подсветки тайла
    private GameObject highlightInstance; // Экземпляр подсветки
    private Vector3Int currentCellPosition; // Текущая позиция тайла
    public Vector2Int offset; // Смещение для изменения тайлов
    private float lastChangeTime; // Время последнего изменения тайлов
    public float changeInterval = 0.1f; // Интервал между изменениями тайлов
    public float prefabDuration = 5f; // Время, через которое префаб исчезнет
    private bool isButtonActive = false; // Состояние кнопки

    // Публичное поле для настройки значения z
    public float mouseWorldZPosition = 1.0f; // Значение z для позиционирования мыши

    // Словарь для хранения оригинальных тайлов
    private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>();

    public enum Element
    {
        Fire,
        Water,
        Earth,
        Air
    }


    void Start()
    {
        highlightInstance = Instantiate(highlightPrefab); // Создаем экземпляр подсветки
        highlightInstance.SetActive(false); // Скрываем подсветку по умолчанию
    }

    void Update()
    {
        // Переключение состояния кнопки с помощью клавиши, например, "Space"
        if (Input.GetKeyDown(KeyCode.Alpha1)) // Замените на нужную клавишу
        {
            ToggleButton(); // Переключаем состояние
        }
        // Если кнопка не активна, скрываем подсветку
        if (!isButtonActive)
        {
            highlightInstance.SetActive(false);
            return; // Выходим из метода
        }
        if (!isButtonActive) return; // Если кнопка не активна, выходим из метода

        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane));
        mouseWorldPosition.z = mouseWorldZPosition; // Используем значение из инспектора
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
                ChangeSoilTilesInArea(currentCellPosition, offset);
                lastChangeTime = Time.time; // Обновляем время последнего изменения
            }
        }
    }

    public void ToggleButton() // Метод для переключения состояния кнопки
    {
        isButtonActive = !isButtonActive; // Переключаем состояние 
        highlightInstance.SetActive(isButtonActive); // Устанавливаем состояние подсветки в зависимости от кнопки
    }

    void ChangeSoilTilesInArea(Vector3Int centerPosition, Vector2Int offset)
    {
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                // Применяем смещение к позиции
                Vector3Int position = new Vector3Int(centerPosition.x + x + offset.x, centerPosition.y + y + offset.y, centerPosition.z);

                // Проверяем, есть ли на позиции земля (используйте ваш метод проверки)
                if (tilemap.HasTile(position) && tilemap.GetTile(position) is TileBase) // Здесь нужно заменить на вашу проверку на тайл земли
                {
                    // Сохраняем оригинальный тайл
                    TileBase originalTile = tilemap.GetTile(position);
                    originalTiles[position] = originalTile; // Сохраняем оригинальный тайл в словаре
                    tilemap.SetTile(position, null); // Удаляем тайл
                }

                // Устанавливаем префаб на ту же позицию
                Vector3 worldPosition = tilemap.GetCellCenterWorld(position);
                GameObject prefabInstance = Instantiate(prefabToPlace, worldPosition, Quaternion.identity); // Устанавливаем префаб

                // Запускаем корутину для восстановления тайла и удаления префаба
                StartCoroutine(RestoreTileAfterDelay(position, prefabInstance));
            }
        }
    }
    private IEnumerator RestoreTileAfterDelay(Vector3Int position, GameObject prefabInstance)
{
    // Ждем указанное время перед восстановлением тайла
    yield return new WaitForSeconds(prefabDuration);

    // Удаляем префаб
    Destroy(prefabInstance);

    // Восстанавливаем оригинальный тайл, если он существует
    if (originalTiles.TryGetValue(position, out TileBase originalTile))
    {
        tilemap.SetTile(position, originalTile); // Восстанавливаем оригинальный тайл
        originalTiles.Remove(position); // Удаляем запись из словаря
    }
}
}