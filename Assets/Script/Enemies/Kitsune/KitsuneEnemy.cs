using UnityEngine;
using Mirror;
using System.Collections;

public class KitsuneEnemy : NetworkBehaviour
{
    [Header("Основные параметры")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float movementAreaRadius = 5f;
    
    [Header("Теневой Клык")]
    [SerializeField] private float shadowFangCooldown = 10f;
    [SerializeField] private int shadowFangDamage = 15;
    [SerializeField] private float curseChance = 0.3f;
    [SerializeField] private float curseDuration = 3f;
    [SerializeField] private float teleportDistance = 2f;
    
    [Header("Проклятие Безмолвия")]
    [SerializeField] private float silenceCooldown = 15f;
    [SerializeField] private float silenceDuration = 5f;
    [SerializeField] private float silenceRange = 6f;
    
    [Header("Призыв Теней")]
    [SerializeField] private GameObject shadowWolfPrefab;
    [SerializeField] private int wolvesToSummon = 3;
    [SerializeField] private float summonCooldown = 20f;
    
    [Header("Фаза Тьмы")]
    [SerializeField] private float darkPhaseCooldown = 25f;
    [SerializeField] private float darkPhaseDuration = 5f;
    [SerializeField] private GameObject darkPhaseEffect;
    [SerializeField] private GameObject shadowTrailPrefab;
    
    [Header("Визуальные эффекты")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject curseEffectPrefab;
    
    private Transform targetPlayer;
    private Rigidbody2D rb;
    private Vector2 movementDirection;
    private Vector2 movementCenter;
    private float nextShadowFangTime;
    private float nextSilenceTime;
    private float nextSummonTime;
    private float nextDarkPhaseTime;
    private bool isInDarkPhase;
    private bool isCasting;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Фиксируем центр зоны при появлении врага
        movementCenter = transform.position;
        
        nextShadowFangTime = Time.time + shadowFangCooldown * 0.5f;
        nextSilenceTime = Time.time + silenceCooldown * 0.7f;
        nextSummonTime = Time.time + summonCooldown * 0.6f;
        nextDarkPhaseTime = Time.time + darkPhaseCooldown * 0.8f;
    }

    [Server]
    private void Update()
    {
        if (isCasting || isInDarkPhase) return;

        FindNearestPlayer();
        
        if (targetPlayer != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.position);
            
            // Проверка способностей
            CheckAbilities(distanceToPlayer);
            
            // Движение в зоне
            MoveInArea(distanceToPlayer);
        }
        else
        {
            IdleMovement();
        }
    }

    [Server]
    private void FindNearestPlayer()
    {
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, detectionRange);
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (Collider2D player in players)
        {
            if (player.CompareTag("Player"))
            {
                float distance = Vector2.Distance(transform.position, player.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPlayer = player.transform;
                }
            }
        }

        targetPlayer = closestPlayer;
    }

    [Server]
    private void MoveInArea(float distanceToPlayer)
    {
        if (distanceToPlayer > detectionRange * 0.5f)
        {
            // Движение к игроку, но в пределах своей зоны
            Vector2 directionToPlayer = (targetPlayer.position - (Vector3)movementCenter).normalized;
            Vector2 targetPoint = movementCenter + directionToPlayer * Mathf.Min(movementAreaRadius, distanceToPlayer * 0.5f);
            
            movementDirection = (targetPoint - (Vector2)transform.position).normalized;
        }
        else
        {
            // Круговое движение вокруг центра зоны
            Vector2 centerDirection = (movementCenter - (Vector2)transform.position).normalized;
            movementDirection = new Vector2(-centerDirection.y, centerDirection.x);
        }

        rb.linearVelocity = movementDirection * moveSpeed;
        RpcUpdateAnimation(movementDirection);
    }

    [Server]
    private void IdleMovement()
    {
        // Случайное блуждание в зоне
        if (Random.value < 0.02f)
        {
            movementDirection = Random.insideUnitCircle.normalized;
        }
        
        // Ограничение движения в пределах зоны
        if (Vector2.Distance(transform.position, movementCenter) > movementAreaRadius)
        {
            movementDirection = (movementCenter - (Vector2)transform.position).normalized;
        }

        rb.linearVelocity = movementDirection * moveSpeed * 0.5f;
        RpcUpdateAnimation(movementDirection);
    }

    [ClientRpc]
    private void RpcUpdateAnimation(Vector2 direction)
    {
        if (direction.magnitude > 0.1f)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
        }
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }

    [Server]
    private void CheckAbilities(float distanceToPlayer)
    {
        if (Time.time > nextShadowFangTime && distanceToPlayer < detectionRange)
        {
            ShadowFangAttack();
            nextShadowFangTime = Time.time + shadowFangCooldown;
        }
        
        if (Time.time > nextSilenceTime && distanceToPlayer < silenceRange)
        {
            SilenceCurse();
            nextSilenceTime = Time.time + silenceCooldown;
        }
        
        if (Time.time > nextSummonTime && distanceToPlayer < detectionRange * 0.7f)
        {
            SummonShadowWolves();
            nextSummonTime = Time.time + summonCooldown;
        }
        
        if (Time.time > nextDarkPhaseTime)
        {
            StartDarkPhase();
            nextDarkPhaseTime = Time.time + darkPhaseCooldown;
        }
    }
    [TargetRpc]
    private void TargetApplyCurse(NetworkConnection target, float duration)
    {
        PlayerStats localStats = connectionToClient.identity.GetComponent<PlayerStats>();
        if (localStats != null)
        {
            localStats.RpcApplyTemporarySlow(0.5f, duration);
            ShowCurseEffect(localStats.transform.position);
        }
    }
    [Server]
    private void ShadowFangAttack()
    {
        if (!isServer || targetPlayer == null) return;

        isCasting = true;
        rb.linearVelocity = Vector2.zero;
        RpcPlayAnimation("Attack");
        
        Vector2 teleportPosition = (Vector2)targetPlayer.position - (Vector2)targetPlayer.right * teleportDistance;
        transform.position = teleportPosition;
        
        PlayerStats playerStats = targetPlayer.GetComponent<PlayerStats>();
        if (playerStats != null && playerStats.connectionToClient != null)
        {
            playerStats.TakeHit(shadowFangDamage);
            
            if (Random.value < curseChance)
            {
                TargetApplyCurse(playerStats.connectionToClient, curseDuration);
            }
        }
        
        Invoke(nameof(EndCasting), 1f);
    }
    [Server]
    private void SilenceCurse()
    {
        if (!isServer) return;

        isCasting = true;
        rb.linearVelocity = Vector2.zero;
        RpcPlayAnimation("Cast");
        
        Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, silenceRange);
        foreach (Collider2D player in players)
        {
            if (player.CompareTag("Player"))
            {
                PlayerSkillController skillController = player.GetComponent<PlayerSkillController>();
                if (skillController != null && skillController.connectionToClient != null)
                {
                    TargetApplySilence(skillController.connectionToClient, silenceDuration);
                }
            }
        }
        
        Invoke(nameof(EndCasting), 1.5f);
    }

    [TargetRpc]
    private void TargetApplySilence(NetworkConnection target, float duration)
    {
        PlayerSkillController localController = connectionToClient.identity.GetComponent<PlayerSkillController>();
        if (localController != null)
        {
            localController.RpcBlockSkills(duration);
            ShowSilenceEffect(localController.transform.position);
        }
    }
    
  

    private void ShowCurseEffect(Vector2 position)
    {
        if (curseEffectPrefab != null)
        {
            Instantiate(curseEffectPrefab, position, Quaternion.identity);
        }
    }

    private void ShowSilenceEffect(Vector2 position)
    {
        if (curseEffectPrefab != null)
        {
            GameObject effect = Instantiate(curseEffectPrefab, position, Quaternion.identity);
            effect.GetComponent<SpriteRenderer>().color = Color.blue;
        }
    }
    
    [Server]
    private void SummonShadowWolves()
    {
        isCasting = true;
        rb.linearVelocity = Vector2.zero;
        RpcPlayAnimation("Summon");
        
        for (int i = 0; i < wolvesToSummon; i++)
        {
            Vector2 spawnPos = movementCenter + Random.insideUnitCircle * movementAreaRadius;
            GameObject wolf = Instantiate(shadowWolfPrefab, spawnPos, Quaternion.identity);
            NetworkServer.Spawn(wolf);
            
            ShadowWolf wolfScript = wolf.GetComponent<ShadowWolf>();
            if (wolfScript != null && targetPlayer != null)
            {
                wolfScript.SetTarget(targetPlayer);
            }
        }
        
        Invoke(nameof(EndCasting), 2f);
    }

    [Server]
    private void StartDarkPhase()
    {
        isInDarkPhase = true;
        rb.linearVelocity = Vector2.zero;
        RpcStartDarkPhase();
        
        // Делаем неуязвимой
        GetComponent<Collider2D>().enabled = false;
        
        // Создаем следы
        StartCoroutine(CreateShadowTrails());
        
        Invoke(nameof(EndDarkPhase), darkPhaseDuration);
    }

    [Server]
    private IEnumerator CreateShadowTrails()
    {
        float endTime = Time.time + darkPhaseDuration;
        while (Time.time < endTime)
        {
            RpcCreateShadowTrail(transform.position);
            yield return new WaitForSeconds(0.3f);
            
            // Перемещение во время фазы, но в пределах зоны
            if (targetPlayer != null)
            {
                Vector2 direction = ((Vector2)targetPlayer.position - movementCenter).normalized;
                Vector2 targetPoint = movementCenter + direction * Mathf.Min(movementAreaRadius * 0.8f, 
                    Vector2.Distance(movementCenter, targetPlayer.position));
                
                Vector2 newPos = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * 0.5f * Time.deltaTime);
                rb.MovePosition(newPos);
            }
        }
    }

    [Server]
    private void EndDarkPhase()
    {
        isInDarkPhase = false;
        GetComponent<Collider2D>().enabled = true;
        RpcEndDarkPhase();
        
        // Возвращаемся в зону, если вышли за ее пределы
        if (Vector2.Distance(transform.position, movementCenter) > movementAreaRadius)
        {
            Vector2 returnDirection = (movementCenter - (Vector2)transform.position).normalized;
            transform.position = movementCenter + returnDirection * movementAreaRadius * 0.9f;
        }
    }

    [Server]
    private void EndCasting()
    {
        isCasting = false;
    }

    [ClientRpc]
    private void RpcPlayAnimation(string triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName);
        }
    }

    [ClientRpc]
    private void RpcShowCurseEffect(Vector2 position)
    {
        Instantiate(curseEffectPrefab, position, Quaternion.identity);
    }

    [ClientRpc]
    private void RpcShowSilenceEffect(Vector2 position)
    {
        GameObject effect = Instantiate(curseEffectPrefab, position, Quaternion.identity);
        effect.GetComponent<SpriteRenderer>().color = Color.blue;
    }

    [ClientRpc]
    private void RpcStartDarkPhase()
    {
        darkPhaseEffect.SetActive(true);
        spriteRenderer.color = new Color(1, 1, 1, 0.3f);
    }

    [ClientRpc]
    private void RpcEndDarkPhase()
    {
        darkPhaseEffect.SetActive(false);
        spriteRenderer.color = Color.white;
    }

    [ClientRpc]
    private void RpcCreateShadowTrail(Vector2 position)
    {
        Instantiate(shadowTrailPrefab, position, Quaternion.identity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(Application.isPlaying ? movementCenter : transform.position, movementAreaRadius);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}