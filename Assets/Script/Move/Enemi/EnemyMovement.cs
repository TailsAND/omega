using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Zone size")]
    public Vector2 zoneSize; // Размер зоны в виде вектора (x, y)
    [Header("Time between passive moving")]
    public float minMoveTime = 1f; // Минимальное время между движениями
    public float maxMoveTime = 3f; // Максимальное время между движениями
    public float speed = 2f; // Скорость движения врага
    private Transform player; // Ссылка на объект игрока
    [Header("Trigger radius")]
    public float chaseRadius = 3f; // Радиус преследования игрока
    public float attackRadius = 1.5f; // Радиус атаки игрока
    public float outerRadius;
    public float chaseTimer = 2f; // Время для обратного отсчета после выхода игрока из радиуса

    private Vector2 targetPosition; // Целевая позиция врага
    private Vector2 currentPosition; // Текущая позиция врага

    private bool isMoving = false; // Флаг, указывающий, движется ли враг
    private bool isChasing = false; // Флаг, указывающий, преследует ли враг игрока

    void Start()
    {
        currentPosition = transform.position; // Запоминаем текущую позицию врага
        MoveToRandomPosition();
        player = GameObject.FindWithTag("Player").transform;
        outerRadius = attackRadius * 2;
    }

    void Update()
    {
        if (isChasing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            // Проверяем расстояние до игрока
            if (distanceToPlayer < attackRadius)
            {
                // Убегаем от игрока, если он слишком близко
                Vector2 directionAwayFromPlayer = (transform.position - player.position).normalized;
                targetPosition = (Vector2)transform.position + directionAwayFromPlayer * chaseRadius;
            }
            else
            {
                // Преследуем игрока, если он на расстоянии
                targetPosition = player.position;

                // Потом когда нибудь owo
                //Invoke("MoveToRandomPositionWhileAttacking", Random.Range(minMoveTime,maxMoveTime));
                //targetPosition = MoveToRandomPositionWhileAttacking();
                //Debug.Log(targetPosition);


            }

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

            // Проверяем, вышел ли игрок за пределы радиуса преследования
            if (distanceToPlayer > chaseRadius)
            {
                StartCoroutine(StopChasing()); // Запускаем таймер, если игрок вышел из радиуса
            }
        }
        else
        {
            // Проверяем расстояние до игрока для начала преследования
            if (Vector2.Distance(transform.position, player.position) < chaseRadius)
            {
                isChasing = true; // Начинаем преследовать игрока
            }
            else if (isMoving)
            {
                // Перемещаем врага к целевой позиции
                transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

                // Проверяем, достигли ли мы целевой позиции
                if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
                {
                    float randomTime = Random.Range(minMoveTime, maxMoveTime);
                    isMoving = false; // Останавливаем движение
                    Invoke("MoveToRandomPosition", randomTime); // Запускаем движение к новой позиции
                }
            }
        }
    }

    void MoveToRandomPosition()
    {
        targetPosition = new Vector2(
            Random.Range(currentPosition.x - zoneSize.x / 2, currentPosition.x + zoneSize.x / 2),
            Random.Range(currentPosition.y - zoneSize.y / 2, currentPosition.y + zoneSize.y / 2) // Новая координата Y
        );

        isMoving = true; // Устанавливаем флаг перемещения
    }

    // Может быть реализую потом когда нибудь
    Vector2 MoveToRandomPositionWhileAttacking()
    {
        Vector2 previousTargetPosition = Vector2.zero; // Хранит предыдущее значение
        float proximityThreshold = 1.5f; // Порог близости

        Vector2 GenerateTargetPosition()
        {
            float randomDistance = Random.Range(attackRadius, outerRadius);
            Vector2 randomDirection = Random.insideUnitCircle * 55;

            Vector2 newTargetPosition = (Vector2)transform.position + randomDirection * randomDistance;

            // Проверка на близость
            if (Vector2.Distance(newTargetPosition, previousTargetPosition) < proximityThreshold)
            {
                // Если близко, генерируем новое значение
                return GenerateTargetPosition();
            }

            // Обновляем предыдущее значение
            previousTargetPosition = newTargetPosition;
            targetPosition = newTargetPosition;
            return newTargetPosition;
        }
        isMoving = true; // Устанавливаем флаг перемещения
        return GenerateTargetPosition();

    }

    private IEnumerator StopChasing()
    {
        yield return new WaitForSeconds(chaseTimer); // Ждем окончания обратного отсчета
        isChasing = false; // Прекращаем преследование
        MoveToRandomPosition(); // Возвращаемся в зону
    }
}