using Mirror;
using UnityEngine;

public class PlayerCombat : NetworkBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private float _meleeAttackRange = 2f;
    [SerializeField] private float _meleeAttackAngle = 90f;
    [SerializeField] private float _rangedAttackAngle = 30f;
    [SerializeField] private float _baseAttackCooldown = 1f;
    [SerializeField] private int _baseDamage = 10;
    [SerializeField] private LayerMask _attackableLayers;
    [SerializeField] private Color _meleeZoneColor = new Color(1, 0, 0, 0.3f);
    [SerializeField] private Color _rangedZoneColor = new Color(0, 0, 1, 0.3f);
    
    [Header("Projectile Prefabs")]
    [SerializeField] private GameObject _arrowPrefab;
    [SerializeField] private GameObject _magicProjectilePrefab;
    [SerializeField] private GameObject _crossbowBoltPrefab;
    [SerializeField] private Transform _projectileSpawnPoint;

    [Header("Damage Info UI")]
    [SerializeField] private GameObject _damageInfoPrefab;
    [SerializeField] private Vector3 _damageInfoOffset = new Vector3(0, 2f, 0);
    
    public int BaseDamage {
        get => _baseDamage;
        set => _baseDamage = value;
    }
    private float _lastAttackTime;
    private PlayerEquipment _equipment;
    private PlayerStats _stats;
    private Animator _animator;
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _equipment = GetComponent<PlayerEquipment>();
        _stats = GetComponent<PlayerStats>();
        _animator = GetComponent<Animator>();
        _playerMovement = GetComponent<PlayerMovement>();

        if (_equipment == null) Debug.LogError("PlayerEquipment not found!");
        if (_stats == null) Debug.LogError("PlayerStats not found!");
        if (_animator == null) Debug.LogWarning("Animator not found - animations won't work");
        if (_playerMovement == null) Debug.LogError("PlayerMovement not found!");
    }

    private void Start()
    {
        if (_arrowPrefab == null) Debug.LogError("Arrow Prefab not assigned!");
        if (_magicProjectilePrefab == null) Debug.LogError("Magic Projectile Prefab not assigned!");
        if (_crossbowBoltPrefab == null) Debug.LogWarning("Crossbow Bolt Prefab not assigned");
        if (_projectileSpawnPoint == null) Debug.LogError("Projectile Spawn Point not assigned!");
    }

    private void Update()
    {
        if (!isLocalPlayer) return;
        
        if (Input.GetButtonDown("Fire1"))
        {
            TryAttack();
        }
    }


    // Визуализация зон атаки в редакторе
    private void OnDrawGizmosSelected()
    {
        // Получаем текущее направление взгляда игрока
        Vector2 lookDirection = _playerMovement != null ? _playerMovement.lastDirection : Vector2.up;
        Vector3 forward = new Vector3(lookDirection.x, lookDirection.y, 0) * _meleeAttackRange;

        // Меле зона
        Gizmos.color = _meleeZoneColor;
        Vector3 left = Quaternion.Euler(0, 0, -_meleeAttackAngle/2) * forward;
        Vector3 right = Quaternion.Euler(0, 0, _meleeAttackAngle/2) * forward;
        
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawLine(transform.position + left, transform.position + forward);
        Gizmos.DrawLine(transform.position + right, transform.position + forward);

        // Дальняя зона (конус)
        Gizmos.color = _rangedZoneColor;
        forward = new Vector3(lookDirection.x, lookDirection.y, 0) * _meleeAttackRange * 2f;
        left = Quaternion.Euler(0, 0, -_rangedAttackAngle/2) * forward;
        right = Quaternion.Euler(0, 0, _rangedAttackAngle/2) * forward;
        
        Gizmos.DrawLine(transform.position, transform.position + left);
        Gizmos.DrawLine(transform.position, transform.position + right);
        Gizmos.DrawLine(transform.position + left, transform.position + forward);
        Gizmos.DrawLine(transform.position + right, transform.position + forward);
    }

    private void TryAttack()
    {
        if (_equipment == null || _stats == null || _playerMovement == null) 
        {
            Debug.LogWarning("Combat components not ready");
            return;
        }

        if (Time.time < _lastAttackTime + GetCurrentAttackCooldown()) return;
        
        _lastAttackTime = Time.time;
        
        try 
        {
            if (_equipment.GetAllItems().TryGetValue(ItemType.Weapon, out var weaponConfig))
            {
                if (weaponConfig != null)
                {
                    PerformWeaponAttack(weaponConfig);
                }
                else
                {
                    Debug.LogWarning("Weapon config is null");
                    PerformFistAttack();
                }
            }
            else
            {
                PerformFistAttack();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Attack failed: {e.Message}");
            PerformFistAttack();
        }
    }

    private void PerformWeaponAttack(ItemConfig weaponConfig)
    {
        if (weaponConfig == null) return;

        ItemData weaponData = _equipment.GetItemData(ItemType.Weapon);
        int damage = weaponData?.attackBonus ?? _baseDamage; // Используем _baseDamage вместо weaponConfig.baseDamage

        switch (weaponConfig.weaponType)
        {
            case WeaponType.Sword:
            case WeaponType.Axe:
            case WeaponType.Dagger:
                MeleeAttack(damage);
                break;
        
            case WeaponType.Bow:
                if (_arrowPrefab != null)
                    RangedAttack(_arrowPrefab, damage, _rangedAttackAngle);
                break;
            
            case WeaponType.Crossbow:
                if (_crossbowBoltPrefab != null)
                    RangedAttack(_crossbowBoltPrefab, damage, _rangedAttackAngle);
                break;
            
            case WeaponType.Staff:
            case WeaponType.Wand:
                if (_magicProjectilePrefab != null)
                    RangedAttack(_magicProjectilePrefab, damage, _rangedAttackAngle);
                break;
            
            default:
                PerformFistAttack();
                break;
        }
    
        if (_animator != null)
        {
            _animator.SetTrigger(GetAttackAnimation(weaponConfig.weaponType));
        }
    }

    private string GetAttackAnimation(WeaponType weaponType)
    {
        return weaponType switch
        {
            WeaponType.Sword => "SwordAttack",
            WeaponType.Axe => "AxeAttack",
            WeaponType.Dagger => "DaggerAttack",
            WeaponType.Bow => "BowAttack",
            WeaponType.Crossbow => "CrossbowAttack",
            WeaponType.Staff => "StaffAttack",
            WeaponType.Wand => "WandAttack",
            _ => "Punch"
        };
    }
    
    [Command]
    private void RangedAttack(GameObject projectilePrefab, int damage, float attackAngle)
    {
        if (projectilePrefab == null || _projectileSpawnPoint == null || _playerMovement == null) 
        {
            Debug.LogWarning("Ranged attack components missing");
            return;
        }

        // Получаем направление взгляда игрока
        Vector2 lookDirection = _playerMovement.lastDirection.normalized;
        Vector3 spawnPosition = _projectileSpawnPoint.position;
    
        // Создаем снаряд
        GameObject projectile = Instantiate(
            projectilePrefab, 
            spawnPosition, 
            Quaternion.identity);
    
        // Направляем снаряд в нужную сторону
        projectile.transform.up = lookDirection;
    
        ProjectileBasicAttack proj = projectile.GetComponent<ProjectileBasicAttack>();
        if (proj != null)
        {
            proj.SetDamage(damage);
            proj.SetOwner(gameObject);
        }
    
        NetworkServer.Spawn(projectile);
    }

    [ClientRpc]
    private void RpcShowProjectile(GameObject projectile)
    {
        // Можно добавить визуальные эффекты для клиентов
    }

    [Command]
    private void MeleeAttack(int damage)
    {
        if (_playerMovement == null) return;

        // Получаем направление взгляда игрока
        Vector2 lookDirection = _playerMovement.lastDirection.normalized;
        Vector2 attackCenter = (Vector2)transform.position + lookDirection * (_meleeAttackRange / 2);
        float attackRadius = _meleeAttackRange / 2;
    
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackCenter, attackRadius, _attackableLayers);
    
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject == gameObject) continue;
        
            Vector2 directionToTarget = ((Vector2)hitCollider.transform.position - (Vector2)transform.position).normalized;
            float angleToTarget = Vector2.Angle(lookDirection, directionToTarget);
        
            // Проверяем, что цель находится в пределах угла атаки
            if (angleToTarget > _meleeAttackAngle / 2) continue;
        
            if (hitCollider.TryGetComponent<TestenemyHealth>(out var enemyHealth))
            {
                int actualDamage = CalculateDamage(damage, enemyHealth.CurrentArmor);
                enemyHealth.TakeDamage(actualDamage, _stats);
                ShowDamageInfo(hitCollider.transform.position, actualDamage);
            }
            else if (hitCollider.TryGetComponent<PlayerStats>(out var playerStats))
            {
                playerStats.TakeHit(damage);
                ShowDamageInfo(hitCollider.transform.position, damage);
            }
        }
    }

    private int CalculateDamage(int baseDamage, int targetArmor)
    {
        // Простая формула расчета урона с учетом брони
        return Mathf.Max(1, baseDamage - targetArmor / 2);
    }

    [ClientRpc]
    private void ShowDamageInfo(Vector3 position, int damage)
    {
        if (_damageInfoPrefab == null) return;
        
        Vector3 spawnPosition = position + _damageInfoOffset;
        GameObject damageInfo = Instantiate(_damageInfoPrefab, spawnPosition, Quaternion.identity);
        DamagePopup popup = damageInfo.GetComponent<DamagePopup>();
        
        if (popup != null)
        {
            popup.SetDamage(damage);
        }
        else
        {
            Destroy(damageInfo);
        }
    }

    [Command]
    private void PerformFistAttack()
    {
        Vector3 attackCenter = transform.position + transform.forward * (_meleeAttackRange / 2);
        float attackRadius = _meleeAttackRange / 2;
        
        Collider[] hitColliders = Physics.OverlapSphere(attackCenter, attackRadius, _attackableLayers);
        
        foreach (var hitCollider in hitColliders)
        {
            Vector3 directionToTarget = (hitCollider.transform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            
            if (angleToTarget > _meleeAttackAngle / 2) continue;
            if (hitCollider.gameObject == gameObject) continue;
            
            if (hitCollider.TryGetComponent<TestenemyHealth>(out var enemyHealth))
            {
                int actualDamage = CalculateDamage(_baseDamage, enemyHealth.CurrentArmor);
                enemyHealth.TakeDamage(actualDamage, _stats);
                ShowDamageInfo(hitCollider.transform.position, actualDamage);
            }
            else if (hitCollider.TryGetComponent<PlayerStats>(out var playerStats))
            {
                playerStats.TakeHit(_baseDamage);
                ShowDamageInfo(hitCollider.transform.position, _baseDamage);
            }
        }
        
        if (_animator != null)
        {
            _animator.SetTrigger("Punch");
        }
    }

    private float GetCurrentAttackCooldown()
    {
        return _baseAttackCooldown;
    }
}