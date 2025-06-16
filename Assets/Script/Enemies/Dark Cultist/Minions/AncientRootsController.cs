using UnityEngine;
using Mirror;
using System.Collections;

public class AncientRootsController : MinionController
{
    [Header("Настройки Древних Корней")]
    [SerializeField] private float _rootDuration = 3f; // Длительность обездвиживания
    [SerializeField] private float _rootCooldown = 8f; // Время между атаками
    [SerializeField] private float _rootRange = 2.5f; // Дистанция атаки
    [SerializeField] private GameObject _rootEffectPrefab; // Эффект опутывания
    [SerializeField] private float _fireDamageMultiplier = 2f; // Множитель урона от огня
    
    private float _lastRootTime;
    private bool _isRooting = false;
    private GameObject _currentRootEffect;

    protected override void Awake()
    {
        base.Awake();
        _moveSpeed *= 0.3f; 
        fireResistance = 0.5f;
    }

    [ServerCallback]
    protected override void Update()
    {
        base.Update();

        if (!_isRooting && 
            Time.time > _lastRootTime + _rootCooldown && 
            _target != null && 
            Vector2.Distance(transform.position, _target.position) <= _rootRange)
        {
            TryRootPlayer();
        }
    }

    [Server]
    private void TryRootPlayer()
    {
        if (_target == null || !_target.CompareTag("Player")) return;

        _lastRootTime = Time.time;
        _isRooting = true;

        // Применяем root эффект
        PlayerMovement playerMovement = _target.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplyRoot(_rootDuration);
            RpcPlayRootEffect(_target.position, true);
        }

        StartCoroutine(EndRootAfterDelay(_rootDuration));
    }

    [Server]
    private IEnumerator EndRootAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _isRooting = false;
        RpcPlayRootEffect(_target.position, false);
    }

    [ClientRpc]
    private void RpcPlayRootEffect(Vector3 position, bool start)
    {
        if (_rootEffectPrefab == null) return;

        if (start)
        {
            // Создаем эффект на всех клиентах
            _currentRootEffect = Instantiate(_rootEffectPrefab, position, Quaternion.identity);
            _currentRootEffect.transform.SetParent(_target); // Прикрепляем к цели
        }
        else if (_currentRootEffect != null)
        {
            // Уничтожаем эффект на всех клиентах
            Destroy(_currentRootEffect);
            _currentRootEffect = null;
        }
    }

    [Server]
    public override void TakeDamage(int damage, DamageType damageType = DamageType.Physical)
    {
        if (damageType == DamageType.Fire)
        {
            damage = Mathf.RoundToInt(damage * _fireDamageMultiplier);
        }

        base.TakeDamage(damage, damageType);
    }

    protected override void HandleMovement()
    {
        if (_target != null && !_isRooting)
        {
            Vector2 direction = (_target.position - transform.position).normalized;
            _rb.linearVelocity = direction * _moveSpeed;
        }
        else
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }

    protected override void Attack()
    {
        // Атака реализована через TryRootPlayer
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _rootRange);
    }
}