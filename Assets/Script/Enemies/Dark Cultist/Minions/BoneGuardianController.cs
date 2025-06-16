using UnityEngine;
using Mirror;
using System.Collections;

public class BoneGuardianController : MinionController
{
    [Header("Настройки Костяного Стража")]
    [SerializeField] private float _blockChance = 0.4f; // 40% шанс блокировки
    [SerializeField] private float _blockDamageReduction = 0.7f; // Уменьшение урона при блоке
    [SerializeField] private float _healthRegenDelay = 3f; // Время без урона для регена
    [SerializeField] private float _healthRegenRate = 5f; // Скорость восстановления HP
    [SerializeField] private GameObject _blockEffectPrefab; // Эффект при блокировании
    [SerializeField] private GameObject _healEffectPrefab; // Эффект при восстановлении

    private float _lastDamageTime;
    private bool _isRegenerating = false;
    private float _originalMoveSpeed;
    private EnemyHealthBar _healthBar;
    private bool _isBlocking = false;
    protected override void Awake()
    {
        base.Awake();
        _healthBar = GetComponent<EnemyHealthBar>();
        _originalMoveSpeed = _moveSpeed;
        _moveSpeed *= 0.5f; // Стражи двигаются медленнее
        _health.OnDamageTaken += HandleDamageTaken;
        fireResistance = 1.2f;
        holyResistance = 0.8f; 
    }

    
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _health.OnDamageTaken -= HandleDamageTaken;
    }

    

    [ServerCallback]
    protected override void Update()
    {
        base.Update();

        // Проверяем возможность регенерации
        if (!_isRegenerating && Time.time > _lastDamageTime + _healthRegenDelay)
        {
            StartCoroutine(RegenerateHealth());
        }
    }

    [Server]
    private void HandleDamageTaken(float damage)
    {
        _lastDamageTime = Time.time;
        
        // Проверяем блокировку
        if (Random.value <= _blockChance)
        {
            _isBlocking = true;
            float reducedDamage = damage * (1f - _blockDamageReduction);
            _health.ServerHeal(damage - reducedDamage);
            
            RpcPlayBlockEffect();
            RpcUpdateHealthBarColor(true);
            Debug.Log($"Блок! Урон уменьшен с {damage} до {reducedDamage}");

            // Возвращаем обычный цвет через 1 секунду
            Invoke(nameof(ResetBlockingState), 1f);
        }
    }

    [Server]
    private void ResetBlockingState()
    {
        _isBlocking = false;
        RpcUpdateHealthBarColor(false);
    }
    
    [ClientRpc]
    private void RpcUpdateHealthBarColor(bool isBlocking)
    {
        if (_healthBar != null)
        {
            _healthBar.SetBlockingState(isBlocking);
        }
    }
    
    [Server]
    private IEnumerator RegenerateHealth()
    {
        _isRegenerating = true;
        RpcPlayHealEffect(true);

        while (Time.time > _lastDamageTime + _healthRegenDelay && 
               !_health.IsDead && 
               _health.CurrentHealth < _health.MaxHealth)
        {
            _health.ServerHeal(_healthRegenRate * Time.deltaTime);
            yield return null;
        }

        RpcPlayHealEffect(false);
        _isRegenerating = false;
    }

    protected override void HandleMovement()
    {
        // Стражи двигаются медленно и методично
        if (_target != null)
        {
            // Подходим ближе чем обычные миньоны из-за щита
            float desiredDistance = _attackRange * 0.7f;
            float currentDistance = Vector2.Distance(transform.position, _target.position);

            if (currentDistance > desiredDistance)
            {
                Vector2 direction = (_target.position - transform.position).normalized;
                _rb.linearVelocity = direction * _moveSpeed;
            }
            else
            {
                _rb.linearVelocity = Vector2.zero;
                Attack();
            }
        }
    }

    protected override void Attack()
    {
        if (_health.CanAttack())
        {
            _health.ResetAttackCooldown();
            
            // Атака щитом - мощнее но медленнее
            _target.GetComponent<PlayerStats>().TakeHit(_health.CurrentAttack + 25);
        }
    }

    [ClientRpc]
    private void RpcPlayBlockEffect()
    {
        if (_blockEffectPrefab != null)
        {
            Instantiate(_blockEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    [ClientRpc]
    private void RpcPlayHealEffect(bool show)
    {
        if (_healEffectPrefab != null)
        {
            // Реализация эффектов зависит от вашей системы частиц
            // Здесь должен быть код управления эффектом восстановления
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _attackRange * 0.7f);
    }
}