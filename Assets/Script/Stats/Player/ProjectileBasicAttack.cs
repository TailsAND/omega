using Mirror;
using UnityEngine;

public class ProjectileBasicAttack : NetworkBehaviour
{
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _lifeTime = 3f;
    [SerializeField] private int _damage = 10;
    
    private GameObject _owner;
    private Rigidbody2D _rb;

    public GameObject GetOwner() => _owner;
    public int GetDamage() => _damage;
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Destroy(gameObject, _lifeTime);
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    public void SetOwner(GameObject owner)
    {
        _owner = owner;
    }

    private void Start()
    {
        if (isServer)
        {
            _rb.velocity = transform.up * _speed; // В 2D используем transform.up
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other) // Изменено на 2D вариант
    {
        if (other.gameObject == _owner) return;
        
        if (other.TryGetComponent<TestenemyHealth>(out var enemyHealth))
        {
            var ownerStats = _owner.GetComponent<PlayerStats>();
            if (ownerStats != null)
            {
                enemyHealth.TakeDamage(_damage, ownerStats);
            }
        }
        else if (other.TryGetComponent<PlayerStats>(out var playerStats))
        {
            playerStats.TakeHit(_damage);
        }
        
        NetworkServer.Destroy(gameObject);
    }

}