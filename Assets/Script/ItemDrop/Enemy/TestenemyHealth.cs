using System;
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
    public event System.Action<float> OnHealthChangedEvent;

    public override void OnStartServer()
    {
        base.OnStartServer();
        _isDead = false;
        CalculateEnemyLevel();
        ScaleStats();
        _currentHealth = MaxHp;
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
        
        if (_currentHealth <= 0)
        {
            Die(attacker);
        }
    }
    [ClientRpc]
    private void RpcUpdateHealth(int newHealth)
    {
        _currentHealth = newHealth;
    }

    [Server]
    private void Die(PlayerStats attacker)
    {
        if (_isDead) return;
        _isDead = true;

        RpcDieEffects();
        
        if (_enemyLoot != null)
        {
            int lootLevel = attacker != null ? attacker.Lvl : _currentLevel;
            _enemyLoot.DropItem(lootLevel);
        }

        if (attacker != null)
        {
            attacker.AddExperience(_experience);
        }
        
        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcDieEffects()
    {
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
            var owner = projectile.GetOwner(); // Используем метод-геттер
            if (owner != null)
            {
                var ownerStats = owner.GetComponent<PlayerStats>();
                if (ownerStats != null)
                {
                    TakeDamage(projectile.GetDamage(), ownerStats); // Используем метод-геттер
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