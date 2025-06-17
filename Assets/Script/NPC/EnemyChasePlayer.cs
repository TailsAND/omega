using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
public class EnemyChasePlayer : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float minDirectionTime = 1f;
    public float maxDirectionTime = 3f;
    public float minStopTime = 0.5f;
    public float maxStopTime = 2f;
    public float movementRadius = 10f;
    public float detectionRange = 5f;
    public float chaseRange = 8f;

    [Header("Sync Variables")]
    [SyncVar(hook = nameof(OnDirectionChanged))]
    private Vector2 _movementDirection;
    [SyncVar(hook = nameof(OnMovementStateChanged))]
    private bool _isMoving;
    [SyncVar]
    private bool _isChasing;

    private Vector2 _initialPosition;
    private Animator _animator;
    private float _actionTimeRemaining;
    private Transform _playerTransform;

    void Start()
    {
        _initialPosition = transform.position;
        _animator = GetComponent<Animator>();
        
        if (isServer)
        {
            // Находим игрока (предполагаем, что у игрока есть тег "Player")
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
            
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
        // Проверяем, видим ли мы игрока
        if (_playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);
            
            if (distanceToPlayer <= detectionRange)
            {
                _isChasing = true;
                ChasePlayer();
                return;
            }
            else if (distanceToPlayer > chaseRange)
            {
                _isChasing = false;
            }
        }

        // Если не преследуем игрока, продолжаем обычное поведение
        if (!_isChasing)
        {
            _actionTimeRemaining -= Time.deltaTime;

            if (_actionTimeRemaining <= 0)
            {
                StartNewAction();
            }

            if (_isMoving)
            {
                MoveEnemy();
            }
        }
    }

    [Server]
    private void ChasePlayer()
    {
        if (_playerTransform == null) return;
        
        Vector2 directionToPlayer = ((Vector2)_playerTransform.position - (Vector2)transform.position).normalized;
        _movementDirection = directionToPlayer;
        _isMoving = true;
        
        MoveEnemy();
    }

    [Server]
    private void MoveEnemy()
    {
        Vector2 newPosition = (Vector2)transform.position + _movementDirection * moveSpeed * Time.deltaTime;
        
        // Ограничиваем движение в пределах радиуса от начальной позиции
        if (!_isChasing && Vector2.Distance(_initialPosition, newPosition) > movementRadius)
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
            _animator.SetBool("IsChasing", _isChasing);
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

    // Визуализация зоны обнаружения в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_initialPosition, movementRadius);
    }
}