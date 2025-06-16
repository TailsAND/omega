using UnityEngine;
using Mirror;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;
    public Animator animator;
    public AudioSource footstepAudio;
    public AudioClip[] footstepSounds;
    [SerializeField] private float footstepInterval = 0.3f;
    
    private Vector2 movementInput;
    public Vector2 lastDirection;
    private Camera mainCamera;
    private float originalMoveSpeed;
    private Coroutine slowCoroutine;
    private Coroutine rootCoroutine;
    private Coroutine footstepCoroutine;
    private bool isFearEffectActive;
    private bool isMoving;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        lastDirection = Vector2.down;
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
        moveSpeed = 0f;
    
        if (isLocalPlayer)
        {
            rb.linearVelocity = Vector2.zero;
        }
    
        yield return new WaitForSeconds(duration);
    
        moveSpeed = originalSpeed;
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            if (isMoving && footstepSounds.Length > 0)
            {
                int index = Random.Range(0, footstepSounds.Length);
                footstepAudio.PlayOneShot(footstepSounds[index]);
            }
            yield return new WaitForSeconds(footstepInterval);
        }
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    void OnDestroy()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        HandleInput();
        UpdateAnimator();
        UpdateCamera();
        UpdateMovementState();
    }

    private void HandleInput()
    {
        if (isFearEffectActive) return;

        movementInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (movementInput.magnitude > 0.1f)
        {
            lastDirection = movementInput;
        }
    }

    private void UpdateMovementState()
    {
        isMoving = movementInput.magnitude > 0.1f && moveSpeed > 0.1f;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Horizontal", lastDirection.x);
        animator.SetFloat("Vertical", lastDirection.y);
        animator.SetFloat("Speed", movementInput.magnitude);
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