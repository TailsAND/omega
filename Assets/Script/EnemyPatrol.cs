using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyPatrol : MonoBehaviour
{
    public Transform[] patrolPoints; // Массив точек патрулирования
    public float speed = 2f; // Скорость движения врага
    public float waitTime = 5f; // Время ожидания на каждой точке
    public float detectionRange = 5f; // Дистанция обнаружения игрока
    public Transform player; // Ссылка на игрока
    public float chaseDuration = 5f; // Время преследования перед переходом на сцену боя
    private int currentPointIndex = 0; // Индекс текущей точки патрулирования
    private bool isChasing = false; // Флаг для отслеживания состояния преследования
    private Animator animator; // Ссылка на компонент Animator

    private void Start()
    {
        animator = GetComponent<Animator>(); // Получаем компонент Animator
        StartCoroutine(Patrol());
    }

    private void Update()
    {
        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            DetectPlayer();
        }

        // Обновляем анимацию в зависимости от скорости
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        float currentSpeed = 0f;
        float horizontal = 0f;
        float vertical = 0f;

        if (!isChasing)
        {
            // Если враг патрулирует, используем направление к текущей точке патрулирования
            if (patrolPoints.Length > 0)
            {
                Transform targetPoint = patrolPoints[currentPointIndex];
                Vector2 direction = (targetPoint.position - transform.position).normalized;
                currentSpeed = (targetPoint.position - transform.position).magnitude > 0.1f ? speed : 0f;

                // Устанавливаем значения horizontal и vertical
                horizontal = direction.x;
                vertical = direction.y;

                transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, currentSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Если враг преследует игрока
            currentSpeed = speed;
            Vector2 direction = (player.position - transform.position).normalized;

            // Устанавливаем значения horizontal и vertical
            horizontal = direction.x;
            vertical = direction.y;

            transform.position = Vector2.MoveTowards(transform.position, player.position, currentSpeed * Time.deltaTime);
        }

        // Устанавливаем параметры Speed, Horizontal и Vertical в Animator
        animator.SetFloat("Speed", currentSpeed);
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
    }


    private void DetectPlayer()
    {
        // Проверяем расстояние до игрока
        if (Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            isChasing = true; // Начинаем преследование
            StartCoroutine(ChaseTimer()); // Запускаем таймер преследования
        }
    }

    private void ChasePlayer()
    {
        // Двигаемся к игроку
        if (Vector2.Distance(transform.position, player.position) < 0.1f)
        {
            // Переход на сцену боя
            SceneManager.LoadScene("Battle scene"); // Замените "BattleScene" на имя вашей сцены боя
        }
    }

    private IEnumerator Patrol()
    {
        while (!isChasing)
        {
            if (patrolPoints.Length == 0) yield break;

            Transform targetPoint = patrolPoints[currentPointIndex];
            while (Vector2.Distance(transform.position, targetPoint.position) > 0.1f)
            {
                yield return null; // Ждем следующего кадра
            }

            // Останавливаем движение
            yield return new WaitForSeconds(waitTime); // Ждем время ожидания
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length; // Циклический переход
        }
    }

    private IEnumerator ChaseTimer()
    {
        yield return new WaitForSeconds(chaseDuration);
        isChasing = false; // Останавливаем преследование после времени
        StartCoroutine(Patrol()); // Возвращаемся к патрулированию
    }
}
