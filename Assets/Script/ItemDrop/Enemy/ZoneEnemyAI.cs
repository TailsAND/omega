using Mirror;
using UnityEngine;

public class ZoneEnemyAI : NetworkBehaviour
{
    [Header("Настройки передвижения")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _patrolRadius = 10f;
    [SerializeField] private float _minIdleTime = 1f;
    [SerializeField] private float _maxIdleTime = 3f;
    
    [Header("Настройки атаки")]
    [SerializeField] private float _attackRange = 3f;
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private int _attackDamage = 10;
    [SerializeField] private string _playerTag = "Player";
    
    private TestenemyHealth _enemyHealth;
    private float _lastAttackTime;
    private Transform _currentTarget;
    private Vector2 _startPosition;
    private Vector2 _currentPatrolPoint;
    private float _idleTimer;
    private float _currentIdleTime;
    private bool _isChasing;

    private void Awake()
    {
        _enemyHealth = GetComponent<TestenemyHealth>();
        _startPosition = transform.position;
        SetNewPatrolPoint();
        _currentIdleTime = Random.Range(_minIdleTime, _maxIdleTime);
    }

    private void Update()
    {
        if (!isServer) {
            return;
        }
        
        if (_enemyHealth.IsDead) {
            Debug.Log("Im dead");
            return;
        }

        FindNearestPlayerByTag();

        if (_currentTarget != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, _currentTarget.position);

            if (distanceToPlayer <= _attackRange)
            {
                Attack();
                return;
            }

            if (distanceToPlayer <= _patrolRadius * 1.5f) 
            {
                ChasePlayer();
                _isChasing = true;
                return;
            }
        }

        _isChasing = false;
        Patrol();
    }

    [Server]
    private void FindNearestPlayerByTag()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag(_playerTag);
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < closestDistance && distance <= _patrolRadius * 1.5f)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
        }

        _currentTarget = closestPlayer;
    }

    [Server]
    private void Patrol()
    {
        if (Vector2.Distance(transform.position, _currentPatrolPoint) < 0.5f)
        {
            _idleTimer += Time.deltaTime;
            
            if (_idleTimer >= _currentIdleTime)
            {
                SetNewPatrolPoint();
                _idleTimer = 0f;
                _currentIdleTime = Random.Range(_minIdleTime, _maxIdleTime);
            }
        }
        else
        {
            Vector2 direction = (_currentPatrolPoint - (Vector2)transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, _currentPatrolPoint, _moveSpeed * Time.deltaTime);
            
            // Поворот спрайта в направлении движения (для 2D)
            if (direction.x != 0)
            {
                // Меняем scale по X для отражения спрайта
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

    [Server]
    private void ChasePlayer()
    {
        if (_currentTarget == null) return;

        Vector2 toPlayer = (Vector2)_currentTarget.position - _startPosition;
        if (toPlayer.magnitude > _patrolRadius)
        {
            Debug.Log("Stop chasing player");
            toPlayer = toPlayer.normalized * _patrolRadius;
            _currentTarget = null;
            return;
        }

        Vector2 direction = ((Vector2)_currentTarget.position - (Vector2)transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, _currentTarget.position, _moveSpeed * Time.deltaTime);
        
        // Поворот спрайта в направлении игрока (для 2D)
        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(direction.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    [Server]
    private void SetNewPatrolPoint()
    {
        Debug.Log("Set new patrol point");
        Vector2 randomPoint = Random.insideUnitCircle * _patrolRadius;
        _currentPatrolPoint = _startPosition + randomPoint;
    }

    [Server]
    private bool CanAttack()
    {
        return _currentTarget != null && 
               Vector2.Distance(transform.position, _currentTarget.position) <= _attackRange &&
               Time.time > _lastAttackTime + _attackCooldown;
    }

    [Server]
    private void Attack()
    {
        if (!CanAttack()) return;
        Debug.Log("Attack");

        Vector2 dir = (Vector2)_currentTarget.position - (Vector2)transform.position;
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(dir.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        
        PlayerStats playerStats = _currentTarget.GetComponent<PlayerStats>();
        if (playerStats != null)
        {
            playerStats.TakeHit(_attackDamage);
            _lastAttackTime = Time.time;
            RpcPlayAttackEffects();
        }
    }

    [ClientRpc]
    private void RpcPlayAttackEffects()
    {
        // Здесь можно добавить визуальные/звуковые эффекты атаки
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(Application.isPlaying ? (Vector3)_startPosition : transform.position, _patrolRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}