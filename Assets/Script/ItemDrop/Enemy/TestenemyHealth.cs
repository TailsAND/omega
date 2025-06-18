using System;
using System.Collections;
using Mirror;
using UnityEngine;
using System.Linq;

public class TestenemyHealth : NetworkBehaviour
{
    [Header("Базовые характеристики")]
    [SerializeField] private int _baseHealth = 100;     
    [SerializeField] private int _baseAttack = 10;       
    [SerializeField] private int _baseArmor = 5;        
    [SerializeField] private float _attackRange = 2f;   
    [SerializeField] private float _attackCooldown = 2f;
    [SerializeField] private int _experience = 200;

    [Header("Масштабирование по уровню")]
    [SerializeField] private int _maxLevelDifference = 3; 
    [SerializeField] private int _healthPerLevel = 20;  
    [SerializeField] private int _attackPerLevel = 3;   
    [SerializeField] private int _armorPerLevel = 1;    
    
    [Header("Ссылки")]
    [SerializeField] private EnemyLoot _enemyLoot;      
    [SerializeField] private GameObject _deathEffectPrefab; 
    
    [Header("Health Bar")]
    [SerializeField] private bool showHealthBar = true;
    
    [Header("Анимации")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _damageAnimationTrigger = "TakeDamage";
    [SerializeField] private string _deathAnimationTrigger = "Die";
    [SerializeField] private float _deathAnimationDuration = 1f;
    [SyncVar] private bool _isPlayingDeath; // Добавляем новый SyncVar
    
    [SyncVar] private int _currentHealth;   
    [SyncVar] private int _currentLevel;    
    [SyncVar] private int _currentAttack;    
    [SyncVar] private int _currentArmor;   
    [SyncVar] private float _lastAttackTime; 
    [SyncVar] private bool _isDead;         
    
    private PlayerStats _lastAttacker;    
    public System.Action OnDeath;          
    public System.Action<float> OnDamageTaken;
    
    #region Свойства
    public int MaxHp => _baseHealth + (_healthPerLevel * (_currentLevel - 1));
    public int CurrentHealth => _currentHealth;
    public int CurrentAttack => _currentAttack;
    public int CurrentArmor => _currentArmor;
    public int Level => _currentLevel;
    public float AttackRange => _attackRange;
    public float AttackCooldown => _attackCooldown;
    public float LastAttackTime => _lastAttackTime;
    public bool IsDead => _isDead;
    public float MaxHealth => MaxHp;
    #endregion
    
    public event Action<float> OnHealthChanged;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _isDead = false;
        CalculateEnemyLevel();
        ScaleStats();
        _currentHealth = MaxHp;
        enabled = true; 
    }

    [Server]
    public void ServerHeal(float amount) 
    {
        if (_isDead) return;
        
        _currentHealth = Mathf.Min(_currentHealth + Mathf.RoundToInt(amount), MaxHp);
        RpcUpdateHealth(_currentHealth);
    }

    [Server]
    protected virtual void ServerUpdate()
    {
        // Base enemy update logic (if any)
    }

    [ServerCallback]
    protected virtual void Update()
    {
        ServerUpdate();
    }

    [Server]
    private void CalculateEnemyLevel()
    {
        var players = FindObjectsOfType<PlayerStats>();
        
        if (players.Length == 0)
        {
            _currentLevel = 1;
            return;
        }

        float averageLevel = (float)players.Average(p => p.Lvl);
        int minLevel = players.Min(p => p.Lvl);

        _currentLevel = Mathf.FloorToInt(Mathf.Min(averageLevel, minLevel + _maxLevelDifference));
        _currentLevel = Mathf.Max(1, _currentLevel);
    }

    [Server]
    private void ScaleStats()
    {
        _currentAttack = _baseAttack + _attackPerLevel * (_currentLevel - 1);
        _currentArmor = _baseArmor + _armorPerLevel * (_currentLevel - 1);
    }

    [Server]
    public void TakeDamage(int damage, PlayerStats attacker)
    {
        if (_isDead) return;

        int actualDamage = Mathf.Max(1, damage - _currentArmor);
        _currentHealth -= actualDamage;
        _lastAttacker = attacker;

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            RpcPlayDamageAnimation();
        }

        RpcUpdateHealth(_currentHealth);
        OnDamageTaken?.Invoke(actualDamage);
        OnHealthChanged?.Invoke((float)_currentHealth / MaxHp); // Добавляем вызов события
    }

    
    [Server]
    private void Die()
    {
        if (_isDead || _isPlayingDeath) return;

        _isDead = true;
        _isPlayingDeath = true;

        RpcPlayDeathAnimation();
        RpcDieEffects();

        if (_enemyLoot != null)
        {
            int lootLevel = _lastAttacker?.Lvl ?? _currentLevel;
            _enemyLoot.DropItem(lootLevel);
        }

        // Отправляем опыт только последнему атакующему игроку
        if (_lastAttacker != null)
        {
            _lastAttacker.TargetAddExperience(_lastAttacker.connectionToClient, _experience);
        }

        OnDeath?.Invoke();
        StartCoroutine(DestroyAfterAnimation());
    }

    [ClientRpc]
    private void RpcPlayDamageAnimation()
    {
        if (!_isPlayingDeath && _animator != null && !string.IsNullOrEmpty(_damageAnimationTrigger))
        {
            _animator.SetTrigger(_damageAnimationTrigger);
        }
    }

    [ClientRpc]
    private void RpcPlayDeathAnimation()
    {
        if (_animator != null)
        {
            _animator.ResetTrigger(_damageAnimationTrigger); // Сбрасываем триггер урона
            _animator.SetTrigger(_deathAnimationTrigger); // Запускаем смерть
        }
    }

    [ClientRpc]
    private void RpcUpdateHealth(int newHealth)
    {
        _currentHealth = newHealth;
        OnHealthChanged?.Invoke((float)_currentHealth / MaxHp); // Вызываем событие и на клиенте
    }

    [Server]
    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(_deathAnimationDuration);
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcDieEffects()
    {
        var collider = GetComponent<Collider>();
        if (collider != null) collider.enabled = false;
        
        if (_deathEffectPrefab != null)
        {
            Instantiate(_deathEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<ProjectileBasicAttack>(out var projectile))
        {
            var owner = projectile.GetOwner();
            if (owner != null)
            {
                var ownerStats = owner.GetComponent<PlayerStats>();
                if (ownerStats != null)
                {
                    TakeDamage(projectile.GetDamage(), ownerStats);
                }
            }
        }
    }

    [Server]
    public bool CanAttack()
    {
        return Time.time > _lastAttackTime + _attackCooldown;
    }

    [Server]
    public void ResetAttackCooldown()
    {
        _lastAttackTime = Time.time;
    }
}