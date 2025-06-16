using UnityEngine;
using Mirror;
using System.Collections;

public class DarkCultistController : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject _darkEnergyProjectile;
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private float _projectileSpeed = 5f;
    [SerializeField] private float _attackDetectionRange = 10f; // Зона обнаружения игрока для атаки
    [SerializeField] private float holyResistance = 1.3f;
   
    
    
    [Header("Minion Settings")]
    [SerializeField] private GameObject[] _minionPrefabs;
    [SerializeField] private int _maxMinions = 3;
    [SerializeField] private float _minionSpawnInterval = 10f;
    [SerializeField] private float _minionLifetime = 30f;
    [SerializeField] private float _minionSpawnDetectionRange = 8f;
    [Header("Curse Settings")]
    [SerializeField] private float _curseRange = 5f;
    [SerializeField] private float _curseDuration = 8f;
    [SerializeField] private float _curseCooldown = 15f;
    [SerializeField] private GameObject _curseEffectPrefab;
    [SerializeField] private float _minionSpawnRadius = 3f;
    [Header("Patrol Settings")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _patrolPointReachedDistance = 0.5f;
    private Vector3[] _patrolPoints;
    private int _currentPatrolIndex = 0;
    private bool _isPatrolling = false;

    [Header("Movement Settings")]
    [SerializeField] private float _movementAreaRadius = 5f;
    [SerializeField] private float _wanderSpeed = 1.5f;
    [SerializeField] private float _wanderChangeDirectionTime = 3f;
    private Vector3 _initialPosition;
    private Vector2 _currentWanderDirection;
    private float _lastWanderDirectionChange;
    
    private TestenemyHealth _health;
    private Transform _target;
    private float _lastCurseTime;
    private float _lastMinionSpawnTime;
    private int _currentMinions;

    private void Awake()
    {
        _health = GetComponent<TestenemyHealth>();
        _initialPosition = transform.position;
        gameObject.layer = LayerMask.NameToLayer("Enemies");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetRandomWanderDirection(); // Перенесли инициализацию направления сюда
        FindTarget();
        StartCoroutine(ServerUpdate());
    }
    

    
    [Server]
    private void SetRandomWanderDirection()
    {
        _currentWanderDirection = Random.insideUnitCircle.normalized;
        _lastWanderDirectionChange = Time.time;
    }
    
    

    [Server]
    private void FindTarget()
    {
        PlayerStats[] players = FindObjectsOfType<PlayerStats>();
        if (players.Length > 0)
        {
            // Находим ближайшего игрока в радиусе обнаружения
            float closestDistance = float.MaxValue;
            foreach (var player in players)
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < _attackDetectionRange && distance < closestDistance)
                {
                    _target = player.transform;
                    closestDistance = distance;
                }
            }
        }
    }
    [Server]
    private void HandleWandering()
    {
        // Меняем направление через случайные интервалы
        if (Time.time > _lastWanderDirectionChange + _wanderChangeDirectionTime)
        {
            SetRandomWanderDirection();
        }

        // Двигаемся в текущем направлении
        Vector3 newPosition = transform.position + (Vector3)_currentWanderDirection * _wanderSpeed * Time.deltaTime;

        // Ограничиваем движение в пределах зоны
        if (Vector2.Distance(newPosition, _initialPosition) > _movementAreaRadius)
        {
            // Разворачиваемся к центру если вышли за границы
            _currentWanderDirection = (_initialPosition - transform.position).normalized;
            newPosition = transform.position + (Vector3)_currentWanderDirection * _wanderSpeed * Time.deltaTime;
        }

        transform.position = newPosition;
    }
    
    
    [Server]
    private IEnumerator ServerUpdate()
    {
        while (!_health.IsDead)
        {
            if (_target == null)
            {
                FindTarget();
                
                // Если цель не найдена, просто бродим
                if (_target == null)
                {
                    HandleWandering();
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
            }

            float distanceToTarget = Vector2.Distance(transform.position, _target.position);

            // Проверяем, находится ли цель все еще в радиусе обнаружения
            if (distanceToTarget > _attackDetectionRange)
            {
                _target = null;
                continue;
            }

            HandleMovement(distanceToTarget);

            if (CanUseCurse() && distanceToTarget <= _curseRange)
            {
                TryCastCurse();
            }

            if (CanSpawnMinions() && distanceToTarget <= _minionSpawnDetectionRange)
            {
                TrySpawnMinions();
            }

            // Атакуем, если игрок в зоне обнаружения (attackDetectionRange)
            if (_health.CanAttack() && distanceToTarget <= _attackDetectionRange)
            {
                TryRangedAttack();
                yield return new WaitForSeconds(_health.AttackCooldown);
            }

            yield return null;
        }
    }
    
    [Server]
    private void HandleMovement(float distanceToTarget)
    {
        if (_target == null)
        {
            HandleWandering();
            return;
        }

        float distanceToInitial = Vector2.Distance(transform.position, _initialPosition);

        if (distanceToInitial > _movementAreaRadius)
        {
            Vector2 direction = (_initialPosition - transform.position).normalized;
            transform.position += (Vector3)direction * 2f * Time.deltaTime;
            return;
        }

        if (distanceToTarget < _health.AttackRange * 0.8f)
        {
            Vector2 direction = (transform.position - _target.position).normalized;
            transform.position += (Vector3)direction * 2f * Time.deltaTime;
        }
        else if (distanceToTarget > _health.AttackRange * 1.2f)
        {
            Vector2 direction = (_target.position - transform.position).normalized;
            transform.position += (Vector3)direction * 2f * Time.deltaTime;
        }
    }


    [Server]
    private Vector2 GetRandomSpawnPosition()
    {
        Vector2 randomPoint = Random.insideUnitCircle * _minionSpawnRadius;
        return (Vector2)transform.position + randomPoint;
    }

    [Server]
    private bool CanUseCurse()
    {
        return Time.time > _lastCurseTime + _curseCooldown && 
               Vector2.Distance(transform.position, _target.position) <= _curseRange;
    }
    [Server]
    private void TryCastCurse()
    {
        if (_target == null) return;

        _lastCurseTime = Time.time;
        RpcPlayCurseAnimation();
        
        NetworkIdentity targetIdentity = _target.GetComponent<NetworkIdentity>();
        if (targetIdentity != null && targetIdentity.connectionToClient != null)
        {
            TargetApplyPainCurse(targetIdentity.connectionToClient);
        }
    }

    [TargetRpc]
    private void TargetApplyPainCurse(NetworkConnection target)
    {
        if (target == null || target.identity == null) return;

        PlayerStats player = target.identity.GetComponent<PlayerStats>();
        if (player != null)
        {
            StartCoroutine(PainCurseEffect(player));
        }
    }

    private IEnumerator PainCurseEffect(PlayerStats player)
    {
        GameObject effect = Instantiate(_curseEffectPrefab, player.transform);
        player.Armor = (int)(player.Armor * 0.75f);

        yield return new WaitForSeconds(_curseDuration);

        player.Armor = (int)(player.Armor / 0.75f);
        if (effect != null) Destroy(effect);
    }

    [Server]
    private bool CanSpawnMinions()
    {
        return _currentMinions < _maxMinions && 
               Time.time > _lastMinionSpawnTime + _minionSpawnInterval;
    }

    [Server]
    private void TrySpawnMinions()
    {
        if (_minionPrefabs == null || _minionPrefabs.Length == 0)
        {
            Debug.LogWarning("No minion prefabs assigned!");
            return;
        }

        _lastMinionSpawnTime = Time.time;
        RpcPlaySummonAnimation();

        for (int i = 0; i < _maxMinions - _currentMinions; i++)
        {
            Vector2 spawnPosition = GetRandomSpawnPosition();
        
            // Выбираем случайный префаб миньона
            GameObject randomMinionPrefab = _minionPrefabs[Random.Range(0, _minionPrefabs.Length)];
            GameObject minion = Instantiate(randomMinionPrefab, spawnPosition, Quaternion.identity);
        
            // Устанавливаем культиста как хозяина для миньона
            MinionController minionController = minion.GetComponent<MinionController>();
            if (minionController != null)
            {
                minionController.SetCultist(transform);
            }
        
            NetworkServer.Spawn(minion);
            _currentMinions++;

            StartCoroutine(DestroyMinionAfterTime(minion, _minionLifetime));
        }
    }
    
    [Server]
    public void TakeDamage(int damage, DamageType damageType = DamageType.Physical)
    {
        float resistance = damageType switch
        {
            DamageType.Holy => holyResistance,
            _ => 1f
        };

        int finalDamage = Mathf.RoundToInt(damage * resistance);
        _health.TakeDamage(finalDamage, null);
        
        Debug.Log($"Cultist took {finalDamage} ({damageType}, resistance: {resistance})");
    }
    [Server]
    private IEnumerator DestroyMinionAfterTime(GameObject minion, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (minion != null)
        {
            NetworkServer.Destroy(minion);
            _currentMinions--;
        }
    }

    [Server]
    private void TryRangedAttack()
    {
        if (_target == null || _projectileSpawnPoint == null || _darkEnergyProjectile == null) 
        {
            Debug.LogWarning("Attack conditions not met");
            return;
        }

        Debug.Log($"Attacking player! Distance: {Vector2.Distance(transform.position, _target.position)}");
    
        _health.ResetAttackCooldown();
        RpcPlayAttackAnimation();

        Vector2 direction = (_target.position - _projectileSpawnPoint.position).normalized;
        GameObject projectile = Instantiate(_darkEnergyProjectile, _projectileSpawnPoint.position, Quaternion.identity);
    
        // Поворачиваем снаряд в направлении игрока
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    
        NetworkServer.Spawn(projectile);

        ProjectileDarkCultist projectileScript = projectile.GetComponent<ProjectileDarkCultist>();
        if (projectileScript != null)
        {
            projectileScript.Initialize(_health.CurrentAttack, direction);
        }
    }
    [ClientRpc]
    private void RpcPlayAttackAnimation()
    {
        // Анимация атаки
    }

    [ClientRpc]
    private void RpcPlaySummonAnimation()
    {
        // Анимация призыва
    }

    [ClientRpc]
    private void RpcPlayCurseAnimation()
    {
        // Анимация проклятия
    }

    private void OnDrawGizmosSelected()
    {
        // Зона спавна миньонов
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _minionSpawnRadius);

        // Зона передвижения
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_initialPosition, _movementAreaRadius);

        // Зона обнаружения для атаки
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackDetectionRange);

        // Зона для спавна миньонов
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _minionSpawnDetectionRange);
        
        if (_patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in _patrolPoints)
            {
                Gizmos.DrawSphere(point, 0.3f);
            }
        }
        
    }
}