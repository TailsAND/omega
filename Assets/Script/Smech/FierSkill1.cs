using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;

public class FierSkil1 : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
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
    public TileBase stoneTile; // Тайл льда
    public TileBase fireTile; // Тайл огня
    public TileBase waterTile; // Тайл воды
    // Словарь для хранения оригинальных тайлов
    private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>();
    void Start()
    {
        highlightInstance = Instantiate(highlightPrefab); // Создаем экземпляр подсветки
        highlightInstance.SetActive(false); // Скрываем подсветку по умолчанию
    }

    void Update()
    {
        // Переключение состояния кнопки с помощью клавиши, например, "Space"
        if (Input.GetKeyDown(KeyCode.Alpha2)) // Замените на нужную клавишу
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
                // Проверяем, есть ли на позиции тайл
                if (tilemap.HasTile(position) && tilemap.GetTile(position) is TileBase)
                {
                    TileBase originalTile = tilemap.GetTile(position);

                    // Условие для исключения огня и воды
                    if (originalTile != fireTile && originalTile != waterTile && originalTile != stoneTile)
                    {
                        originalTiles[position] = originalTile; // Сохраняем оригинальный тайл в словаре
                        Debug.Log($"Original tile saved at {position}: {originalTile}");
                    }

                 
                }

                // Устанавливаем новый тайл
                tilemap.SetTile(position, fireTile); // Устанавливаем новый тайл, выбранный в инспекторе

                // Проверка на соседние тайлы для создания воды
                CheckForWaterCreation(position);
            }
        }
    }
    public void CheckForWaterCreation(Vector3Int position)
    {
        bool hasIce = false;
        bool hasFire = false;

        // Проверяем соседние позиции
        Vector3Int[] neighborPositions = {
            new Vector3Int(position.x - 1, position.y, position.z), // Слева
            new Vector3Int(position.x + 1, position.y, position.z), // Справа
            new Vector3Int(position.x, position.y - 1, position.z), // Снизу
            new Vector3Int(position.x, position.y + 1, position.z)  // Сверху
        };

        foreach (var neighbor in neighborPositions)
        {
            if (tilemap.HasTile(neighbor))
            {
                TileBase tile = tilemap.GetTile(neighbor);
                if (tile == waterTile)
                {
                    hasIce = true; // Найден тайл льда
                }
                else if (tile == fireTile)
                {
                    hasFire = true; // Найден тайл огня
                }
            }
        }

        // Если оба тайла найдены, устанавливаем тайл воды
        if (hasIce && hasFire)
        {
            tilemap.SetTile(position, stoneTile); // Устанавливаем тайл воды
        }
        else
        {
            // Запускаем корутину для исчезновения тайла
            StartCoroutine(RestoreTileAfterDelay(position, 3f)); // Задержка 3 секунды перед удалением
        }
    }
    private IEnumerator RestoreTileAfterDelay(Vector3Int position, float delay)
    {
        yield return new WaitForSeconds(delay); // Ждем указанное время

        // Проверяем, является ли тайл водой
        if (originalTiles.TryGetValue(position, out TileBase originalTile))
        {
            tilemap.SetTile(position, originalTile); // Восстанавливаем оригинальный тайл (воду)
            originalTiles.Remove(position); // Удаляем запись из словаря
        }
    }
}