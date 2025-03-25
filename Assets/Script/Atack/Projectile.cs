using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 10f; // Скорость снаряда
    public float lifetime = 2f; // Время жизни снаряда
    private Vector2 direction; // Направление полета снаряда

    void Start()
    {
        // Уничтожаем снаряд через заданное время
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Двигаем снаряд в заданном направлении
        transform.Translate(direction * speed * Time.deltaTime);
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection; // Устанавливаем направление
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Логика при столкновении (например, урон игроку)
        Destroy(gameObject); // Уничтожаем снаряд при столкновении
    }
}
