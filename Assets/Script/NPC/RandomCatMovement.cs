using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
public class RandomCatMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float minDirectionTime = 1f;
    public float maxDirectionTime = 3f;
    public float minStopTime = 0.5f;
    public float maxStopTime = 2f;
    public float movementRadius = 5f;

    [Header("Sync Variables")]
    [SyncVar(hook = nameof(OnDirectionChanged))]
    private Vector2 _movementDirection;
    [SyncVar(hook = nameof(OnMovementStateChanged))]
    private bool _isMoving;

    private Vector2 _initialPosition;
    private Animator _animator;
    private float _actionTimeRemaining;

    void Start()
    {
        _initialPosition = transform.position;
        _animator = GetComponent<Animator>();
        
        if (isServer)
        {
            StartNewAction();
        }
    }

    void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }
        
        UpdateAnimation();
    }

    [Server]
    private void ServerUpdate()
    {
        _actionTimeRemaining -= Time.deltaTime;

        if (_actionTimeRemaining <= 0)
        {
            StartNewAction();
        }

        if (_isMoving)
        {
            MoveCat();
        }
    }

    [Server]
    private void MoveCat()
    {
        Vector2 newPosition = (Vector2)transform.position + _movementDirection * moveSpeed * Time.deltaTime;
        
        if (Vector2.Distance(_initialPosition, newPosition) > movementRadius)
        {
            _movementDirection = (_initialPosition - newPosition).normalized;
            newPosition = (Vector2)transform.position + _movementDirection * moveSpeed * Time.deltaTime;
        }
        
        transform.position = newPosition;
    }

    [Server]
    private void StartNewAction()
    {
        if (Random.value < 0.7f)
        {
            SetRandomDirection();
            _isMoving = true;
            _actionTimeRemaining = Random.Range(minDirectionTime, maxDirectionTime);
        }
        else
        {
            _movementDirection = Vector2.zero;
            _isMoving = false;
            _actionTimeRemaining = Random.Range(minStopTime, maxStopTime);
        }
    }

    [Server]
    private void SetRandomDirection()
    {
        int direction = Random.Range(0, 4);
        _movementDirection = direction switch
        {
            0 => Vector2.up,
            1 => Vector2.right,
            2 => Vector2.down,
            3 => Vector2.left,
            _ => Vector2.zero
        };
    }

    private void UpdateAnimation()
    {
        if (_animator != null)
        {
            _animator.SetFloat("MoveX", _movementDirection.x);
            _animator.SetFloat("MoveY", _movementDirection.y);
            _animator.SetBool("IsMoving", _isMoving);
        }
    }

    private void OnDirectionChanged(Vector2 oldDir, Vector2 newDir)
    {
        _movementDirection = newDir;
    }

    private void OnMovementStateChanged(bool oldState, bool newState)
    {
        _isMoving = newState;
    }
}