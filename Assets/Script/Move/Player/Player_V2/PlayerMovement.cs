using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public float stairSpeedReductionFactor = 0.25f; // Уменьшенный фактор уменьшения скорости на лестнице
    public float ladderClimbSpeed; // Скорость подъема по лестнице (будет вычисляться)
    public Rigidbody2D rb;
    public Animator animator;

    private bool isOnStairs = false; // Флаг, указывающий, находится ли игрок на лестнице
    private Vector2 movement; // Объявляем переменную на уровне класса
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    void Start()
    {
        // Устанавливаем начальную скорость подъема по лестнице
        ladderClimbSpeed = moveSpeed * stairSpeedReductionFactor; // Уменьшаем скорость на фактор
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        // Обнуляем вектор движения
        movement = Vector2.zero;

        // Проверяем нажатие клавиш и задаем направление движения
        if (Input.GetKey(KeyCode.W)) // Вверх
        {
            if (isOnStairs)
            {
                movement.y += 1; // Увеличиваем y при подъеме
            }
            else
            {
                movement.y += 1; // Движение вверх на земле
            }
        }
        if (Input.GetKey(KeyCode.S)) // Вниз
        {
            if (isOnStairs)
            {
                movement.y -= 1; // Уменьшаем y при спуске
            }
            else
            {
                movement.y -= 1; // Движение вниз на земле
            }
        }
        if (Input.GetKey(KeyCode.A)) // Влево
        {
            movement.x -= 1;
        }
        if (Input.GetKey(KeyCode.D)) // Вправо
        {
            movement.x += 1;
        }

        // Нормализуем вектор движения для равномерной скорости
        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        // Умножаем на скорость перемещения
        movement *= moveSpeed;

        // Устанавливаем параметры анимации
        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
        CameraMovement();
    }

    private void CameraMovement()
    {

        mainCam.transform.localPosition = new Vector3(transform.position.x, transform.position.y, -9f);
        transform.position = Vector2.MoveTowards(transform.position,mainCam.transform.localPosition,Time.deltaTime);

    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        // Определяем скорость движения
        float currentSpeed = isOnStairs ? ladderClimbSpeed : moveSpeed;

        // Двигаем игрока
        Vector2 moveDirection = movement.normalized * currentSpeed * Time.fixedDeltaTime;

        // Двигаем игрока
        rb.MovePosition(rb.position + moveDirection);
        CameraMovement();

    }

    // Метод для установки флага, указывающего, находится ли игрок на лестнице
    public void SetOnStairs(bool onStairs)
    {
        isOnStairs = onStairs;
    }
}
