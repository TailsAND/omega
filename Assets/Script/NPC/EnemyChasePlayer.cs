using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformReliable))]
[RequireComponent(typeof(CapsuleCollider2D))]
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

    [Header("Attack Settings")]
    public int attackDamage = 10;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Sync Variables")]
    [SyncVar(hook = nameof(OnDirectionChanged))]
    private Vector2 movementDirection;
    [SyncVar(hook = nameof(OnMovementStateChanged))]
    private bool isMoving;
    [SyncVar]
    private bool isChasing;

    private Vector2 initialPosition;
    private Animator animator;
    private float actionTimeRemaining;
    private PlayerStats playerStats;
    private bool playerInRange;

    void Start()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();
        
        if (isServer)
        {
            // Начинаем поиск игрока
            InvokeRepeating(nameof(FindPlayer), 0f, 2f); // Поиск каждые 2 секунды
            StartNewAction();
        }
    }

    void OnDestroy()
    {
        if (isServer)
        {
            CancelInvoke(nameof(FindPlayer));
        }
    }

    void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }
    }

    [Server]
    private void FindPlayer()
    {
        // Если игрок уже найден и он жив - ничего не делаем
        if (playerStats != null && playerStats.CurrentlyHp > 0) return;

        // Ищем через NetworkManager всех игроков
        foreach (NetworkConnection conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null)
            {
                PlayerStats stats = conn.identity.GetComponent<PlayerStats>();
                if (stats != null && stats.IsPlayer)
                {
                    playerStats = stats;
                    return;
                }
            }
        }

        // Альтернативный способ через поиск по компоненту
        PlayerStats[] allPlayers = FindObjectsOfType<PlayerStats>();
        foreach (PlayerStats ps in allPlayers)
        {
            if (ps.IsPlayer)
            {
                playerStats = ps;
                return;
            }
        }
    }

    [Server]
    private void ServerUpdate()
    {
        // Если игрок не найден или мертв - пропускаем
        if (playerStats == null || playerStats.CurrentlyHp <= 0) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerStats.transform.position);
        
        if (distanceToPlayer <= detectionRange || playerInRange)
        {
            isChasing = true;
            ChasePlayer();
        }
        else if (distanceToPlayer > chaseRange && !playerInRange)
        {
            isChasing = false;
        }

        if (!isChasing)
        {
            actionTimeRemaining -= Time.deltaTime;

            if (actionTimeRemaining <= 0)
            {
                StartNewAction();
            }

            if (isMoving)
            {
                MoveEnemy();
            }
        }
    }

    [Server]
    private void ChasePlayer()
    {
        if (playerStats == null) return;
        
        Vector2 directionToPlayer = ((Vector2)playerStats.transform.position - (Vector2)transform.position).normalized;
        movementDirection = directionToPlayer;
        isMoving = true;
        
        MoveEnemy();
    }
    [Server]
    private void MoveEnemy()
    {
        Vector2 newPosition = (Vector2)transform.position + movementDirection * moveSpeed * Time.deltaTime;
        
        // Ограничиваем движение в пределах радиуса, если не преследуем игрока
        if (!isChasing && Vector2.Distance(initialPosition, newPosition) > movementRadius)
        {
            movementDirection = (initialPosition - newPosition).normalized;
            newPosition = (Vector2)transform.position + movementDirection * moveSpeed * Time.deltaTime;
        }
        
        transform.position = newPosition;
    }

    [Server]
    private void StartNewAction()
    {
        if (Random.value < 0.7f)
        {
            SetRandomDirection();
            isMoving = true;
            actionTimeRemaining = Random.Range(minDirectionTime, maxDirectionTime);
        }
        else
        {
            movementDirection = Vector2.zero;
            isMoving = false;
            actionTimeRemaining = Random.Range(minStopTime, maxStopTime);
        }
    }

    [Server]
    private void SetRandomDirection()
    {
        int direction = Random.Range(0, 4);
        movementDirection = direction switch
        {
            0 => Vector2.up,
            1 => Vector2.right,
            2 => Vector2.down,
            3 => Vector2.left,
            _ => Vector2.zero
        };
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null && ps.IsPlayer)
        {
            playerInRange = true;
            AttackPlayer(ps.gameObject);
        }
    }

    [ServerCallback]
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerStats ps = other.GetComponent<PlayerStats>();
        if (ps != null && ps.IsPlayer)
        {
            playerInRange = false;
        }
    }

    [Server]
    private void AttackPlayer(GameObject player)
    {
        lastAttackTime = Time.time;
        
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.TakeHit(attackDamage);
            stats.RpcPlayHitSound();
        }
        
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    private void OnDirectionChanged(Vector2 oldDir, Vector2 newDir)
    {
        movementDirection = newDir;
    }

    private void OnMovementStateChanged(bool oldState, bool newState)
    {
        isMoving = newState;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(initialPosition, movementRadius);
    }
}