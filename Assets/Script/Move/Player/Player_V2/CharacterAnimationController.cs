using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
    public Animator animator; // Ссылка на компонент Animator
    public Transform player; // Ссылка на персонажа

    private Vector3 lastPosition; // Хранит последнюю позицию персонажа

    void Start()
    {
        lastPosition = player.position; // Инициализируем последнюю позицию
    }

    void Update()
    {
        // Получаем позицию мыши в мировых координатах
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0; // Убираем Z-компонент

        // Вычисляем направление от персонажа к мыши
        Vector3 direction = mousePosition - player.position;

        // Проверяем, движется ли персонаж
        if (player.position != lastPosition)
        {
            // Если персонаж движется, обновляем последнюю позицию
            lastPosition = player.position;
            return; // Выходим из метода, чтобы не обновлять анимацию
        }

        // Обновляем анимацию в зависимости от положения мыши
        UpdateAnimation(direction);
    }

    private void UpdateAnimation(Vector3 direction)
    {
        // Сбрасываем триггеры, чтобы избежать конфликтов
        animator.ResetTrigger("lookUp");
        animator.ResetTrigger("lookDown");
        animator.ResetTrigger("lookRight");
        animator.ResetTrigger("lookLeft");

        // Сравниваем абсолютные значения для определения направления
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            // Горизонтальное направление преобладает
            if (direction.x > 0)
            {
                // Мышь справа от персонажа
                animator.SetTrigger("lookLeft");
            }
            else
            {
                // Мышь слева от персонажа
                animator.SetTrigger("lookRight");
            }
        }
        else
        {
            // Вертикальное направление преобладает
            if (direction.y > 0)
            {
                // Мышь выше персонажа
                animator.SetTrigger("lookUp");
            }
            else
            {
                // Мышь ниже персонажа
                animator.SetTrigger("lookDown");
            }
        }
    }
}