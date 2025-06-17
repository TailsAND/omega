using UnityEngine;
using Mirror;
using System.Collections.Generic;

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
    private readonly List<PlayerStats> playersInRange = new List<PlayerStats>();
    private readonly HashSet<PlayerStats> allPlayers = new HashSet<PlayerStats>();

    private void Start()
    {
        initialPosition = transform.position;
        animator = GetComponent<Animator>();

        if (isServer)
        {
            InvokeRepeating(nameof(UpdatePlayerList), 1f, 2f); // Обновляем список игроков каждые 2 секунды
            StartNewAction();
        }
    }

    private void OnDestroy()
    {
        if (isServer)
        {
            CancelInvoke(nameof(UpdatePlayerList));
        }
    }

    private void Update()
    {
        if (isServer)
        {
            ServerUpdate();
        }

        UpdateAnimation();
    }

    [Server]
    private void UpdatePlayerList()
    {
        allPlayers.Clear();
        
        // 1. Поиск через NetworkManager (основной способ)
        foreach (NetworkConnection conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null && conn.identity.TryGetComponent(out PlayerStats stats))
            {
                allPlayers.Add(stats);
            }
        }

        // 2. Альтернативный поиск по компоненту (на случай если первый способ не сработал)
        if (allPlayers.Count == 0)
        {
            foreach (PlayerStats player in FindObjectsOfType<PlayerStats>())
            {
                if (player.isActiveAndEnabled)
                {
                    allPlayers.Add(player);
                }
            }
        }
    }

    [Server]
    private PlayerStats FindClosestPlayer()
    {
        PlayerStats closestPlayer = null;
        float closestDistance = float.MaxValue;

        foreach (PlayerStats player in allPlayers)
        {
            if (player == null || player.CurrentlyHp <= 0) continue;

            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player;
            }
        }

        return closestPlayer;
    }

    [Server]
    private void ServerUpdate()
    {
        PlayerStats closestPlayer = FindClosestPlayer();
        if (closestPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, closestPlayer.transform.position);

        if (distanceToPlayer <= detectionRange || playersInRange.Count > 0)
        {
            isChasing = true;
            ChasePlayer(closestPlayer);
        }
        else if (distanceToPlayer > chaseRange && playersInRange.Count == 0)
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
    private void ChasePlayer(PlayerStats player)
    {
        if (player == null) return;

        Vector2 directionToPlayer = ((Vector2)player.transform.position - (Vector2)transform.position).normalized;
        movementDirection = directionToPlayer;
        isMoving = true;

        MoveEnemy();
    }

    [Server]
    private void MoveEnemy()
    {
        Vector2 newPosition = (Vector2)transform.position + movementDirection * moveSpeed * Time.deltaTime;

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

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("MoveX", movementDirection.x);
            animator.SetFloat("MoveY", movementDirection.y);
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsChasing", isChasing);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerStats player) && !playersInRange.Contains(player))
        {
            playersInRange.Add(player);
            AttackPlayer(player);
        }
    }

    [ServerCallback]
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerStats player))
        {
            playersInRange.Remove(player);
        }
    }

    [ServerCallback]
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerStats player) && 
            Time.time > lastAttackTime + attackCooldown)
        {
            AttackPlayer(player);
        }
    }

    [Server]
    private void AttackPlayer(PlayerStats player)
    {
        lastAttackTime = Time.time;
        
        if (player != null && player.CurrentlyHp > 0)
        {
            player.TakeHit(attackDamage);
            player.RpcPlayHitSound();
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