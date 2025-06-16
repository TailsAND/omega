using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;
    
    private Vector2 movementInput;
    public Vector2 lastDirection;
    private Camera mainCamera;
    private float originalMoveSpeed;
    private Coroutine slowCoroutine;
    private Coroutine rootCoroutine;
    private bool isFearEffectActive;
    
    
    private void Awake()
    {
        mainCamera = Camera.main;
        lastDirection = Vector2.down; // Начальное направление - вниз
        originalMoveSpeed = moveSpeed;
    }

    [Server]
    public void ApplySlow(float duration, float amount)
    {
        if (!isLocalPlayer) return; 
        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }
        slowCoroutine = StartCoroutine(SlowEffect(duration, amount));
    }

    [ClientRpc]
    public void ApplyRoot(float duration)
    {
        if (!isLocalPlayer) return;
    
        if (rootCoroutine != null)
        {
            StopCoroutine(rootCoroutine);
        }
        rootCoroutine = StartCoroutine(RootEffect(duration));
    }
    [Server]
    public void ApplyFearEffect(float duration)
    {
        TargetApplyFearEffect(duration);
    }
    
    [TargetRpc]
    private void TargetApplyFearEffect(float duration)
    {
        // Запускаем эффект страха (например, инверсия управления)
        StartCoroutine(FearEffect(duration));
    }
    
    private IEnumerator FearEffect(float duration)
    {
        isFearEffectActive = true;
        float endTime = Time.time + duration;
        while (Time.time < endTime)
        {
            movementInput = Random.insideUnitCircle;
            yield return null;
        }
        isFearEffectActive = false;
    }
    
    private IEnumerator SlowEffect(float duration, float amount)
    {
        moveSpeed = originalMoveSpeed * (1f - amount);
        yield return new WaitForSeconds(duration);
        moveSpeed = originalMoveSpeed;
    }

    private IEnumerator RootEffect(float duration)
    {
        float originalSpeed = moveSpeed;
        moveSpeed = 0f; // Полное обездвиживание
    
        // Принудительно останавливаем движение
        if (isLocalPlayer)
        {
            rb.linearVelocity = Vector2.zero;
        }
    
        yield return new WaitForSeconds(duration);
    
        moveSpeed = originalSpeed;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleInput();
        UpdateAnimator();
        UpdateCamera();
    }

    private void HandleInput()
    {
        if (isFearEffectActive) return; // Добавьте флаг isFearEffectActive

        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (movementInput.magnitude > 0.1f)
        {
            lastDirection = movementInput;
        }
    }

    private void UpdateAnimator()
    {
        // Устанавливаем параметры аниматора
        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
        animator.SetFloat("Speed", movementInput.magnitude); // Величина движения (0-1)
    }

    private void UpdateCamera()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                mainCamera.transform.position.z
            );
        }
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        rb.linearVelocity = movementInput * moveSpeed;
    }
}