using UnityEngine;
using Mirror;

[RequireComponent(typeof(NetworkIdentity))]
[RequireComponent(typeof(Rigidbody2D))]
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
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Destroy(gameObject, _lifetime);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!isServer)
        {
            ApplyMovement();
        }
    }

    [Server]
    public void Initialize(int damage, Vector2 direction)
    {
        _syncDamage = damage;
        _direction = direction;
        ApplyMovement();
        
        RpcInitialize(direction);
    }

    [ClientRpc]
    private void RpcInitialize(Vector2 direction)
    {
        if (!isServer)
        {
            _direction = direction;
            ApplyMovement();
        }
    }

    private void ApplyMovement()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = _direction * _speed;
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