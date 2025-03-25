using UnityEngine;

public class FixedPointCollider : MonoBehaviour
{
    public Transform player; // Ссылка на персонажа
    public float radius = 1.5f; // Радиус, на котором будет находиться коллайдер
    public float rotationSpeed = 5f; // Скорость вращения

    private Vector3 fixedPoint; // Фиксированная точка (позиция игрока)

    void Start()
    {
        // Устанавливаем фиксированную точку на позиции игрока
        fixedPoint = player.position;
    }

    void Update()
    {
        // Обновляем фиксированную точку на позиции игрока
        fixedPoint = player.position;

        // Получаем позицию мыши в мировых координатах
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Убираем Z-компонент

        // Вычисляем направление от фиксированной точки к мыши
        Vector3 direction = (mousePosition - fixedPoint).normalized;

        // Находим угол между направлением и вектором вперед персонажа
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Поворачиваем коллайдер к мыши
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Перемещаем коллайдер на фиксированное расстояние от фиксированной точки
        transform.position = fixedPoint + direction * radius;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Обработка столкновений с другими объектами
        if (other.CompareTag("Water"))
        {
            Debug.Log("Collided with Water!");
        }
    }
}