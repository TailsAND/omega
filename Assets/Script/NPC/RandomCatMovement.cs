using UnityEngine;

public class RandomCatMovement : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float minDirectionTime = 1f;
    public float maxDirectionTime = 3f;
    public float minStopTime = 0.5f;
    public float maxStopTime = 2f;
    public float movementRadius = 5f;

    private Vector2 movementDirection;
    private float actionTimeRemaining;
    private Vector2 initialPosition;
    private Animator animator;
    private bool isMoving;

    void Start()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
        StartNewAction();
    }

    void Update()
    {
        actionTimeRemaining -= Time.deltaTime;

        if (actionTimeRemaining <= 0)
        {
            StartNewAction();
        }

        // Если движется - перемещаем
        if (isMoving)
        {
            transform.Translate(movementDirection * moveSpeed * Time.deltaTime);
            
            // Проверяем границы перемещения
            if (Vector2.Distance(initialPosition, transform.position) > movementRadius)
            {
                movementDirection = (initialPosition - (Vector2)transform.position).normalized;
            }
        }

        // Обновляем параметры аниматора
        animator.SetFloat("MoveX", Mathf.Round(movementDirection.x));
        animator.SetFloat("MoveY", Mathf.Round(movementDirection.y));
        animator.SetBool("IsMoving", isMoving);
    }

    void StartNewAction()
    {
        // 70% chance to move, 30% chance to stop
        if (Random.value < 0.7f)
        {
            // Выбираем новое направление
            int direction = Random.Range(0, 4);
            switch (direction)
            {
                case 0: movementDirection = Vector2.up; break;
                case 1: movementDirection = Vector2.right; break;
                case 2: movementDirection = Vector2.down; break;
                case 3: movementDirection = Vector2.left; break;
            }
            isMoving = true;
            actionTimeRemaining = Random.Range(minDirectionTime, maxDirectionTime);
        }
        else
        {
            // Останавливаемся
            movementDirection = Vector2.zero;
            isMoving = false;
            actionTimeRemaining = Random.Range(minStopTime, maxStopTime);
        }
    }
}