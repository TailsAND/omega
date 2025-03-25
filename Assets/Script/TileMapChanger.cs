using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMapChanger : MonoBehaviour
{
    public Tilemap tilemap; // Ссылка на ваш тайлмап
    public Tile newTile; // Новый тайл для замены

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Проверяем, нажата ли левая кнопка мыши
        {
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int Grid = tilemap.WorldToCell(mouseWorldPosition); // Преобразуем в координаты сетки

            ChangeTilesInArea(Grid);
        }
    }

    void ChangeTilesInArea(Vector3Int center)
    {
        // Измените радиус области, если нужно
        int radius = 4;

        for (int x = center.x - radius; x <= center.x + radius; x++)
        {
            for (int y = center.y - radius; y <= center.y + radius; y++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);

                if (tilemap.HasTile(position)) // Проверка наличия тайла на позиции
                {
                    tilemap.SetTile(position, newTile); // Замена тайла
                }
            }
        }
    }
}
