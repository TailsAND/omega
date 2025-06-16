using UnityEngine;
using Mirror;

public class ProjectileDarkCultist : NetworkBehaviour
{
    [SerializeField] private int _damage;
    [SerializeField] private float _lifetime = 3f;
    [SerializeField] private float _speed = 5f;
    private Rigidbody2D _rb;
    
    [SyncVar]
    private Vector2 _direction;
    [SyncVar]
    private int _syncDamage;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemies"), true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Minions"), true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Projectiles"), true);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Mebeli"), true);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Сервер уничтожает снаряд через время жизни
        Destroy(gameObject, _lifetime);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        // Применяем движение на клиентах при получении снаряда
        if (!isServer) // Только клиенты применяют это
        {
            _rb.velocity = _direction * _speed;
        }
    }

    [Server]
    public void Initialize(int damage, Vector2 direction)
    {
        _syncDamage = damage;
        _direction = direction;
        _rb.velocity = direction * _speed;
        
        // Инициализируем на всех клиентах
        RpcInitialize(direction);
    }

    [ClientRpc]
    private void RpcInitialize(Vector2 direction)
    {
        if (!isServer) // Только клиенты применяют это
        {
            _direction = direction;
            _rb.velocity = direction * _speed;
        }
    }

    [ServerCallback]
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isServer) return;

        PlayerStats player = other.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeHit(_syncDamage);
            NetworkServer.Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            NetworkServer.Destroy(gameObject);
        }
    }
}