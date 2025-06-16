using Mirror;
using UnityEngine;
using System.Collections;

public class ShadowConjurer : TestenemyHealth
{
    [Header("Shadow Conjurer Settings")]
    [SerializeField] private float teleportCooldown = 8f;
    [SerializeField] private float teleportRange = 5f;
    [SerializeField] private float invisibilityDuration = 3f;
    [SerializeField] private float curseDuration = 5f;
    [SerializeField] private float curseDamageReduction = 0.5f;
    [SerializeField] private float curseSlowAmount = 0.5f;
    [SerializeField] private GameObject smokeTrailPrefab;
    [SerializeField] private GameObject curseEffectPrefab;
    [SerializeField] private float smokeTrailDuration = 3f;
    
    [Header("Patrol Settings")]
    [SerializeField] private float _patrolSpeed = 1.5f;
    [SerializeField] private float _patrolPointReachedDistance = 0.5f;
    [SerializeField] private float _patrolAreaRadius = 5f;
    [SerializeField] private int _numPatrolPoints = 4;
    private Vector3[] _patrolPoints;
    private int _currentPatrolIndex = 0;
    private Vector3 _initialPosition;
    
    private float nextTeleportTime;
    private float nextInvisibilityTime;
    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private bool isInvisible;
    private Color originalColor;
    private Transform _target;

    public override void OnStartServer()
    {
        base.OnStartServer();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        originalColor = spriteRenderer.color;
        _initialPosition = transform.position;
        
        SetupPatrolPoints();
        nextTeleportTime = Time.time + teleportCooldown;
        nextInvisibilityTime = Time.time + teleportCooldown * 0.7f;
    }
    [Server]
    private void SetupPatrolPoints()
    {
        _patrolPoints = new Vector3[_numPatrolPoints];
        for (int i = 0; i < _numPatrolPoints; i++)
        {
            float angle = i * (360f / _numPatrolPoints);
            Vector3 point = _initialPosition + Quaternion.Euler(0, 0, angle) * Vector3.right * _patrolAreaRadius;
            _patrolPoints[i] = point;
        }
    }

    [Server]
    protected override void ServerUpdate()
    {
        base.ServerUpdate();

        if (IsDead) return;

        if (_target == null)
        {
            FindTarget();
            
            if (_target == null)
            {
                Patrol();
                return;
            }
        }

        HandleAbilities();
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
                if (distance < AttackRange * 2f && distance < closestDistance)
                {
                    _target = player.transform;
                    closestDistance = distance;
                }
            }
        }
    }

    [Server]
    private void Patrol()
    {
        if (_patrolPoints.Length == 0) return;

        Vector3 targetPoint = _patrolPoints[_currentPatrolIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPoint, _patrolSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPoint) < _patrolPointReachedDistance)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }
    }

    [Server]
    private void HandleAbilities()
    {
        if (Time.time > nextTeleportTime)
        {
            PhaseTeleport();
            nextTeleportTime = Time.time + teleportCooldown;
        }

        if (Time.time > nextInvisibilityTime && !isInvisible)
        {
            StartCoroutine(BecomeInvisible());
            nextInvisibilityTime = Time.time + teleportCooldown * 1.5f;
        }
    }

    [Server]
    private void PhaseTeleport()
    {
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector2 teleportPosition = (Vector2)transform.position + randomDirection * teleportRange;
        
        // Ensure teleport stays within patrol area
        if (Vector2.Distance(teleportPosition, _initialPosition) > _patrolAreaRadius)
        {
            teleportPosition = _initialPosition + (Vector3)(teleportPosition - (Vector2)_initialPosition).normalized * _patrolAreaRadius;
        }
        
        RpcCreateSmokeTrail(transform.position);
        transform.position = teleportPosition;
        
        if (Random.value > 0.5f)
        {
            CurseNearbyPlayers();
        }
    }
    
    [ClientRpc]
    private void RpcCreateSmokeTrail(Vector3 position)
    {
        if (smokeTrailPrefab != null)
        {
            GameObject smoke = Instantiate(smokeTrailPrefab, position, Quaternion.identity);
            StartCoroutine(DestroySmokeAfterTime(smoke, smokeTrailDuration));
        }
    }

    private IEnumerator DestroySmokeAfterTime(GameObject smokeEffect, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (smokeEffect != null)
        {
            // Плавное исчезновение
            SpriteRenderer smokeRenderer = smokeEffect.GetComponent<SpriteRenderer>();
            if (smokeRenderer != null)
            {
                float fadeDuration = 0.5f;
                float elapsed = 0f;
                Color originalColor = smokeRenderer.color;
                
                while (elapsed < fadeDuration)
                {
                    elapsed += Time.deltaTime;
                    smokeRenderer.color = new Color(
                        originalColor.r,
                        originalColor.g,
                        originalColor.b,
                        Mathf.Lerp(originalColor.a, 0, elapsed / fadeDuration)
                    );
                    yield return null;
                }
            }
            
            Destroy(smokeEffect);
        }
    }
    
    [Server]
    private IEnumerator BecomeInvisible()
    {
        isInvisible = true;
        RpcSetInvisibility(true);
        
        yield return new WaitForSeconds(invisibilityDuration);
        
        isInvisible = false;
        RpcSetInvisibility(false);
        
        if (CanAttack())
        {
            ResetAttackCooldown();
        }
    }

    [ClientRpc]
    private void RpcSetInvisibility(bool invisible)
    {
        if (invisible)
        {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.3f);
            if (enemyCollider != null) enemyCollider.enabled = false;
        }
        else
        {
            spriteRenderer.color = originalColor;
            if (enemyCollider != null) enemyCollider.enabled = true;
        }
    }

    [Server]
    private void CurseNearbyPlayers()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, AttackRange * 1.5f);
        foreach (var hitCollider in hitColliders)
        {
            PlayerStats player = hitCollider.GetComponent<PlayerStats>();
            if (player != null)
            {
                ApplyCurse(player);
            }
        }
    }

    [Server]
    private void ApplyCurse(PlayerStats player)
    {
        player.TakeHit(Mathf.RoundToInt(CurrentAttack * curseDamageReduction));
        
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.ApplySlow(curseDuration, curseSlowAmount);
        }
        
        RpcShowCurseEffect(player.gameObject);
    }

    [ClientRpc]
    private void RpcShowCurseEffect(GameObject player)
    {
        if (curseEffectPrefab != null)
        {
            Instantiate(curseEffectPrefab, player.transform.position, Quaternion.identity, player.transform);
        }
    }

    [Server]
    public void TakeDamage(int damage, PlayerStats attacker)
    {
        if (isInvisible)
        {
            damage = Mathf.RoundToInt(damage * 0.7f);
        }
        
        base.TakeDamage(damage, attacker);
        
        if (isInvisible)
        {
            StopAllCoroutines();
            isInvisible = false;
            RpcSetInvisibility(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw patrol area
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _patrolAreaRadius);

        // Draw patrol points
        if (_patrolPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var point in _patrolPoints)
            {
                Gizmos.DrawSphere(point, 0.3f);
            }
        }
    }
}