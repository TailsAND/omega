using UnityEngine;
using Mirror;
using System.Collections;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(NetworkTransformUnreliable))]
public class DarkCultistController : NetworkBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private GameObject _darkEnergyProjectile;
    [SerializeField] private Transform _projectileSpawnPoint;
    [SerializeField] private float _projectileSpeed = 5f;
    [SerializeField] private float _attackDetectionRange = 10f;
    [SerializeField] private float holyResistance = 1.3f;

    [Header("Minion Settings")]
    [SerializeField] private GameObject[] _minionPrefabs;
    [SerializeField] private int _maxMinions = 3;
    [SerializeField] private float _minionSpawnInterval = 10f;
    [SerializeField] private float _minionLifetime = 30f;
    [SerializeField] private float _minionSpawnDetectionRange = 8f;
    [SerializeField] private float _minionSpawnRadius = 3f;

    [Header("Curse Settings")]
    [SerializeField] private float _curseRange = 5f;
    [SerializeField] private float _curseDuration = 8f;
    [SerializeField] private float _curseCooldown = 15f;
    [SerializeField] private GameObject _curseEffectPrefab;

    [Header("Movement Settings")]
    [SerializeField] private float _movementAreaRadius = 5f;
    [SerializeField] private float _wanderSpeed = 1.5f;
    [SerializeField] private float _wanderChangeDirectionTime = 3f;

    [SyncVar] private Vector3 _initialPosition;
    [SyncVar] private Vector2 _currentWanderDirection;
    [SyncVar] private float _lastWanderDirectionChange;
    [SyncVar] private Transform _target;
    [SyncVar] private int _currentMinions;

    private TestenemyHealth _health;
    private float _lastCurseTime;
    private float _lastMinionSpawnTime;

    private void Awake()
    {
        _health = GetComponent<TestenemyHealth>();
        _initialPosition = transform.position;
        SetRandomWanderDirection();
        gameObject.layer = LayerMask.NameToLayer("Enemies");
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
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
    private IEnumerator ServerUpdate()
    {
        while (!_health.IsDead)
        {
            if (_target == null)
            {
                FindTarget();
                
                if (_target == null)
                {
                    HandleWandering();
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
            }

            float distanceToTarget = Vector2.Distance(transform.position, _target.position);

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

            if (_health.CanAttack() && distanceToTarget <= _attackDetectionRange)
            {
                TryRangedAttack();
                yield return new WaitForSeconds(_health.AttackCooldown);
            }

            yield return null;
        }
    }

    [Server]
    private void HandleWandering()
    {
        if (Time.time > _lastWanderDirectionChange + _wanderChangeDirectionTime)
        {
            SetRandomWanderDirection();
        }

        Vector3 newPosition = transform.position + (Vector3)_currentWanderDirection * _wanderSpeed * Time.deltaTime;

        if (Vector2.Distance(newPosition, _initialPosition) > _movementAreaRadius)
        {
            _currentWanderDirection = (_initialPosition - transform.position).normalized;
            newPosition = transform.position + (Vector3)_currentWanderDirection * _wanderSpeed * Time.deltaTime;
        }

        transform.position = newPosition;
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
        
            GameObject randomMinionPrefab = _minionPrefabs[Random.Range(0, _minionPrefabs.Length)];
            GameObject minion = Instantiate(randomMinionPrefab, spawnPosition, Quaternion.identity);
        
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
            return;

        _health.ResetAttackCooldown();
        RpcPlayAttackAnimation();

        Vector2 direction = (_target.position - _projectileSpawnPoint.position).normalized;
        GameObject projectile = Instantiate(_darkEnergyProjectile, _projectileSpawnPoint.position, Quaternion.identity);
    
        NetworkServer.Spawn(projectile);

        ProjectileDarkCultist projectileScript = projectile.GetComponent<ProjectileDarkCultist>();
        if (projectileScript != null)
        {
            StartCoroutine(InitializeProjectileNextFrame(projectileScript, direction));
        }
    }

    [Server]
    private IEnumerator InitializeProjectileNextFrame(ProjectileDarkCultist projectile, Vector2 direction)
    {
        yield return null;
        projectile.Initialize(_health.CurrentAttack, direction);
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
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _minionSpawnRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_initialPosition, _movementAreaRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackDetectionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _minionSpawnDetectionRange);
    }
}

