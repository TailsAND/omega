using UnityEngine;
using Mirror;
using System.Collections;

public class StationaryPoisonEnemyController : MinionController
{
    [Header("Настройки Ядовитого Монстра")]
    [SerializeField] private float _slowAmount = 0.5f;
    [SerializeField] private float _poisonDamage = 3f;
    [SerializeField] private float _poisonInterval = 1f;
    [SerializeField] private float _poisonDuration = 5f;
    [SerializeField] private GameObject _poisonPoolPrefab;
    [SerializeField] private float _poolSpawnInterval = 2f;
    [SerializeField] private int _maxPools = 3;
    [SerializeField] private float _poolLifetime = 8f;
    [SerializeField] private float _attackRange = 4f; // Увеличенный радиус атаки для неподвижного врага
    [SerializeField] private LayerMask _poolSpawnMask;
    private float _lastPoolSpawnTime;
    private int _spawnedPoolsCount;

    protected override void Awake()
    {
        base.Awake();
        _moveSpeed = 0f;
        poisonResistance = 0.3f;
        holyResistance = 1.4f;
        fireResistance = 0.8f;
    }

    [ServerCallback]
    protected override void Update()
    {
        base.Update();

        if (CanSpawnPool())
        {
            SpawnPoisonPool();
            _lastPoolSpawnTime = Time.time;
            _spawnedPoolsCount++;
        }
    }
    
    [Server]
    private bool CanSpawnPool()
    {
        return Time.time > _lastPoolSpawnTime + _poolSpawnInterval && 
               _spawnedPoolsCount < _maxPools &&
               _poisonPoolPrefab != null;
    }

    [Server]
    private void SpawnPoisonPool()
    {
        Vector2 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition == Vector2.zero) return;

        GameObject pool = Instantiate(_poisonPoolPrefab, spawnPosition, Quaternion.identity);
        NetworkServer.Spawn(pool);
        
        // Инициализация пула на сервере
        if (pool.TryGetComponent<PoisonPoolController>(out var poolController))
        {
            poolController.RpcPlaySpawnEffect();
        }
        
        StartCoroutine(DestroyPoolAfterTime(pool, _poolLifetime));
    }

    [Server]
    private Vector2 FindValidSpawnPosition()
    {
        const int maxAttempts = 5;
        for (int i = 0; i < maxAttempts; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * _attackRange;
            
            // Проверяем, что точка не внутри коллайдера и доступна для спавна
            if (!Physics2D.OverlapPoint(randomPoint, _poolSpawnMask))
            {
                return randomPoint;
            }
        }
        return Vector2.zero;
    }
    
    [Server]
    private IEnumerator DestroyPoolAfterTime(GameObject pool, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (pool != null)
        {
            NetworkServer.Destroy(pool);
            _spawnedPoolsCount = Mathf.Max(0, _spawnedPoolsCount - 1);
        }
    }
    
    

    // Переопределяем движение - враг неподвижен
    protected override void HandleMovement()
    {
        _rb.linearVelocity = Vector2.zero;
    }


    protected override void Attack()
    {
        base.Attack();
    
        if (_health.CanAttack() && _target != null)
        {
            _health.ResetAttackCooldown();
            PlayerStats player = _target.GetComponent<PlayerStats>();
            if (player != null)
            {
                ApplyPoisonEffect(player);
            }
        }
    }

    [Server]
    private void ApplyPoisonEffect(PlayerStats player)
    {
        if (player == null) return;

        // Применяем эффект сразу на сервере
        StartCoroutine(ServerPoisonEffect(player));

        // Отправляем эффект клиенту
        NetworkIdentity targetIdentity = player.GetComponent<NetworkIdentity>();
        if (targetIdentity != null && targetIdentity.connectionToClient != null)
        {
            TargetApplyPoisonEffect(targetIdentity.connectionToClient);
        }
    }

    [Server]
    private IEnumerator ServerPoisonEffect(PlayerStats player)
    {
        float originalSpeed = player.MovementSpeed;
        player.MovementSpeed *= _slowAmount;

        float endTime = Time.time + _poisonDuration;
        while (Time.time < endTime && player != null)
        {
            player.TakeHit(Mathf.RoundToInt(_poisonDamage));
            yield return new WaitForSeconds(_poisonInterval);
        }

        if (player != null)
        {
            player.MovementSpeed = originalSpeed;
        }
    }

    [TargetRpc]
    private void TargetApplyPoisonEffect(NetworkConnection target)
    {
        if (target == null || target.identity == null) return;

        PlayerStats player = target.identity.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.RpcShowPoisonEffect();
            StartCoroutine(ClientPoisonEffect(player));
        }
    }

    private IEnumerator ClientPoisonEffect(PlayerStats player)
    {
        yield return new WaitForSeconds(_poisonDuration);
        if (player != null)
        {
            player.RpcHidePoisonEffect();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _attackRange);
    }
}