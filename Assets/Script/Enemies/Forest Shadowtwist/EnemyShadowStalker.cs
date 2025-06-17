using UnityEngine;
using Mirror;
using System.Collections;

public class EnemyShadowStalker : NetworkBehaviour
{
    [Header("Shadow Stalker Settings")]
    [SerializeField] private float stealthDuration = 3f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float stealthSpeedMultiplier = 1.5f;
    [SerializeField] private float pounceRange = 5f;
    [SerializeField] private float pounceSpeed = 10f;
    [SerializeField] private float pounceCooldown = 5f;
    [SerializeField] private float postPounceStunDuration = 1f;
    [SerializeField] private float lightVulnerabilityMultiplier = 2f;
    
    [Header("Debuff Settings")]
    [SerializeField] private float debuffInterval = 1f; // Интервал между применениями дебаффа
    [SerializeField] private float debuffDuration = 3f; // Длительность дебаффа
    [SerializeField] private int debuffStacksMax = 3; // Максимальное количество стаков дебаффа
    [SerializeField] private float debuffEffectValue = 0.1f; // Значение эффекта дебаффа (например, снижение скорости)
    [SerializeField] private float bleedingDamagePerTick = 5f;
    [SerializeField] private float bleedingTickInterval = 1f;
    [SerializeField] private float fearDuration = 3f;
    [SerializeField] private float hunterMarkMultiplier = 1.5f;
    
    [Header("Patrol Settings")]
    [SerializeField] private float patrolRadius = 10f; // Радиус зоны патрулирования
    [SerializeField] private float patrolPointReachedThreshold = 0.5f; // Дистанция для смены точки патрулирования
    [SerializeField] private float idleTimeBetweenPatrolPoints = 2f; // Время простоя между точками
    
    [Header("Visual Effects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color stealthColor = new Color(1, 1, 1, 0.3f);
    [SerializeField] private GameObject pounceEffectPrefab;
    
    [Header("Attachment Settings")]
    [SerializeField] private float attachmentDuration = 5f; // Максимальное время прикрепления
    [SerializeField] private float attachmentDistance = 1.5f; // Дистанция прикрепления
    [SerializeField] private float attachmentCheckInterval = 0.2f; // Интервал проверки прикрепления
    
    [Header("Attachment Conditions")]
    [SerializeField] private bool onlyAttachInPatrolZone = true;
    
    private TestenemyHealth health;
    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine stealthCoroutine;
    private Coroutine patrolCoroutine;
    
    [SyncVar] private bool isInStealth;
    [SyncVar] private bool isPouncing;
    [SyncVar] private bool isStunned;
    [SyncVar] private bool isPatrolling;
    
    private float nextPounceTime;
    private Transform currentTarget;
    private Vector2 lastDirection;
    private Vector2 patrolCenter; // Центральная точка патрулирования
    private Vector2 currentPatrolPoint; // Текущая точка патрулирования

    private Coroutine attachmentCoroutine;
    private bool isAttachedToPlayer = false;
    private float attachmentEndTime;
    
    private Coroutine debuffCoroutine;
    private PlayerStats debuffedPlayer;
    
    private void Awake()
    {
        health = GetComponent<TestenemyHealth>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        patrolCenter = transform.position; // Устанавливаем центр патрулирования
        StartStealthCycle();
        StartPatrol();
        health.OnDeath += OnEnemyDeath;
    }

    [Server]
    private void StartPatrol()
    {
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);
        
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    [Server]
    private IEnumerator PatrolRoutine()
    {
        while (!health.IsDead)
        {
            // Выбираем новую случайную точку в радиусе патрулирования
            currentPatrolPoint = patrolCenter + Random.insideUnitCircle * patrolRadius;
            isPatrolling = true;
            
            // Двигаемся к точке
            while (Vector2.Distance(transform.position, currentPatrolPoint) > patrolPointReachedThreshold && 
                   currentTarget == null && !isStunned)
            {
                Vector2 direction = (currentPatrolPoint - (Vector2)transform.position).normalized;
                lastDirection = direction;
                
                float currentSpeed = isInStealth ? 
                    health.CurrentAttack * stealthSpeedMultiplier * 0.5f : 
                    health.CurrentAttack * 0.5f;
                    
                rb.linearVelocity = direction * currentSpeed;
                RpcUpdateAnimator(direction, rb.linearVelocity.magnitude);
                
                yield return null;
            }
            
            // Останавливаемся на некоторое время
            rb.linearVelocity = Vector2.zero;
            RpcUpdateAnimator(Vector2.zero, 0f);
            isPatrolling = false;
            
            yield return new WaitForSeconds(idleTimeBetweenPatrolPoints);
        }
    }

    [Server]
    private void StartStealthCycle()
    {
        if (stealthCoroutine != null)
            StopCoroutine(stealthCoroutine);
        
        stealthCoroutine = StartCoroutine(StealthCycle());
    }

    [Server]
    private IEnumerator StealthCycle()
    {
        while (!health.IsDead)
        {
            // Enter stealth mode
            isInStealth = true;
            RpcSetStealthVisuals(true);
            yield return new WaitForSeconds(stealthDuration);
            
            // Exit stealth mode
            isInStealth = false;
            RpcSetStealthVisuals(false);
            yield return new WaitForSeconds(visibleDuration);
        }
    }

    [ClientRpc]
    private void RpcSetStealthVisuals(bool stealth)
    {
        if (stealth)
        {
            spriteRenderer.color = stealthColor;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
    [Server]
    void Update()
    {
        if (health.IsDead || isStunned || isAttachedToPlayer) return;
        
        FindNearestTarget();
        
        if (currentTarget != null)
        {
            HandleMovement();
            HandlePounce();
        }
    }

    [Server]
    private void FindNearestTarget()
    {
        if (currentTarget != null || isAttachedToPlayer) return;
        
        PlayerStats[] players = FindObjectsOfType<PlayerStats>();
        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;
        
        foreach (PlayerStats player in players)
        {
            if (player.greenZone) continue;
            
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }
        
        currentTarget = closestPlayer;
        if (currentTarget != null && patrolCoroutine != null)
        {
            StopCoroutine(patrolCoroutine);
            isPatrolling = false;
        }
    }

    [Server]
    private void HandleMovement()
    {
        if (isPouncing) return;
        
        Vector2 direction = (currentTarget.position - transform.position).normalized;
        lastDirection = direction;
        
        float currentSpeed = isInStealth ? 
            health.CurrentAttack * stealthSpeedMultiplier : 
            health.CurrentAttack;
            
        rb.linearVelocity = direction * currentSpeed;
        
        RpcUpdateAnimator(direction, rb.linearVelocity.magnitude);
    }

    [Server]
    private void HandlePounce()
    {
        if (Time.time < nextPounceTime || isInStealth) return;
    
        float distanceToTarget = Vector2.Distance(transform.position, currentTarget.position);
        if (distanceToTarget <= pounceRange)
        {
            // Проверяем, находится ли игрок в зоне патрулирования (если включена настройка)
            if (onlyAttachInPatrolZone)
            {
                float distanceToPatrolCenter = Vector2.Distance(currentTarget.position, patrolCenter);
                if (distanceToPatrolCenter > patrolRadius)
                {
                    return; // Игрок вне зоны патрулирования - не прикрепляемся
                }
            }
        
            StartCoroutine(PounceAttack());
        }
    }

    [Server]
    private IEnumerator PounceAttack()
    {
        isPouncing = true;
        nextPounceTime = Time.time + pounceCooldown;
        
        Vector2 originalVelocity = rb.linearVelocity;
        rb.linearVelocity = lastDirection * pounceSpeed;
        
        RpcPlayPounceEffect();
        
        yield return new WaitForSeconds(0.5f); // Pounce duration
        
        // Проверяем, попали ли мы в игрока
        if (currentTarget != null && Vector2.Distance(transform.position, currentTarget.position) < attachmentDistance)
        {
            AttachToPlayer(currentTarget);
        }
        
        // Stun after pounce
        isStunned = true;
        isPouncing = false;
        rb.linearVelocity = Vector2.zero;
        
        yield return new WaitForSeconds(postPounceStunDuration);
        
        isStunned = false;
    }
    
    [Server]
    private void AttachToPlayer(Transform player)
    {
        if (isAttachedToPlayer) return;
    
        PlayerStats playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null || !playerStats.CanAttachEnemy()) return;
    
        isAttachedToPlayer = true;
        attachmentEndTime = Time.time + attachmentDuration;
        playerStats.RegisterAttachedEnemy();
    
        // Остальной код остается без изменений
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);
        isPatrolling = false;
    
        if (stealthCoroutine != null)
            StopCoroutine(stealthCoroutine);
        isInStealth = false;
        RpcSetStealthVisuals(false);
    
        if (attachmentCoroutine != null)
            StopCoroutine(attachmentCoroutine);
        attachmentCoroutine = StartCoroutine(AttachmentRoutine(player));
    
        if (debuffCoroutine != null)
            StopCoroutine(debuffCoroutine);
        debuffedPlayer = playerStats;
        debuffCoroutine = StartCoroutine(ApplyDebuffRoutine());
    }
    
    [Server]
    private IEnumerator AttachmentRoutine(Transform player)
    {
        while (isAttachedToPlayer && Time.time < attachmentEndTime && player != null)
        {
            // Двигаемся к игроку, если он немного отошел
            if (Vector2.Distance(transform.position, player.position) > attachmentDistance * 0.8f)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * health.CurrentAttack * 0.7f;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
            
            RpcUpdateAnimator((player.position - transform.position).normalized, rb.linearVelocity.magnitude);
            yield return new WaitForSeconds(attachmentCheckInterval);
        }
        
        DetachFromPlayer();
    }
    
    [Server]
    private void DetachFromPlayer()
    {
        if (!isAttachedToPlayer) return;
    
        isAttachedToPlayer = false;
    
        if (debuffedPlayer != null)
        {
            debuffedPlayer.UnregisterAttachedEnemy();
        }
    
        // Остальной код остается без изменений
        if (attachmentCoroutine != null)
            StopCoroutine(attachmentCoroutine);
        attachmentCoroutine = null;
    
        if (debuffCoroutine != null)
        {
            StopCoroutine(debuffCoroutine);
            RemoveDebuff();
        }
    
        currentTarget = null;
        StartPatrol();
        StartStealthCycle();
    }
    
    [Server]
    private IEnumerator ApplyDebuffRoutine()
    {
        while (debuffedPlayer != null && Vector2.Distance(transform.position, debuffedPlayer.transform.position) < 2f)
        {
            ApplyDebuff();
            yield return new WaitForSeconds(debuffInterval);
        }
        
        // Когда выходим из цикла, убираем дебафф
        RemoveDebuff();
        debuffCoroutine = null;
    }

    [Server]
    private void ApplyDebuff()
    {
        if (debuffedPlayer == null) return;
    
        // Применяем стандартный дебафф скорости
        ShadowStalkerDebuff debuff = debuffedPlayer.GetComponent<ShadowStalkerDebuff>();
        if (debuff == null)
        {
            debuff = debuffedPlayer.gameObject.AddComponent<ShadowStalkerDebuff>();
        }
        debuff.SetDebuffParameters(debuffDuration, debuffEffectValue, debuffStacksMax);
        debuff.AddStack();
    
        // С шансом 30% накладываем кровотечение
        if (Random.value < 0.3f)
        {
            BleedingDebuff bleeding = debuffedPlayer.GetComponent<BleedingDebuff>();
            if (bleeding == null)
            {
                bleeding = debuffedPlayer.gameObject.AddComponent<BleedingDebuff>();
                bleeding.SetParameters(debuffDuration * 2, bleedingDamagePerTick, bleedingTickInterval);
                bleeding.ApplyDebuff();
            }
        }
    
        // С шансом 20% накладываем страх (если игрок не в зеленой зоне)
        if (Random.value < 0.2f && !debuffedPlayer.greenZone)
        {
            FearDebuff fear = debuffedPlayer.GetComponent<FearDebuff>();
            if (fear == null)
            {
                fear = debuffedPlayer.gameObject.AddComponent<FearDebuff>();
                fear.SetParameters(fearDuration);
                fear.ApplyDebuff();
            }
        }
    
        // Всегда накладываем метку охотника
        HunterMarkBuff mark = debuffedPlayer.GetComponent<HunterMarkBuff>();
        if (mark == null)
        {
            mark = debuffedPlayer.gameObject.AddComponent<HunterMarkBuff>();
            mark.SetParameters(debuffDuration * 3, hunterMarkMultiplier);
            mark.ApplyBuff();
        }
    
        RpcShowDebuffEffect(debuffedPlayer.gameObject);
    }
    [Server]
    private void RemoveDebuff()
    {
        if (debuffedPlayer == null) return;
        
        ShadowStalkerDebuff debuff = debuffedPlayer.GetComponent<ShadowStalkerDebuff>();
        if (debuff != null)
        {
            // Уничтожаем дебафф, если нет стаков
            if (debuff.CurrentStacks <= 0)
            {
                Destroy(debuff);
            }
        }
        
        debuffedPlayer = null;
    }

    [ClientRpc]
    private void RpcShowDebuffEffect(GameObject player)
    {
        // Здесь можно добавить визуальные эффекты для дебаффа
        // Например, частицы или изменение цвета игрока
    }
    
    [ClientRpc]
    private void RpcPlayPounceEffect()
    {
        if (pounceEffectPrefab != null)
        {
            Instantiate(pounceEffectPrefab, transform.position, Quaternion.identity);
        }
    }

    [ClientRpc]
    private void RpcUpdateAnimator(Vector2 direction, float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Horizontal", direction.x);
            animator.SetFloat("Vertical", direction.y);
            animator.SetFloat("Speed", speed);
        }
    }

    [Server]
    public void TakeDamageWithLightCheck(int damage, PlayerStats attacker, bool isLightDamage = false)
    {
        float damageMultiplier = 1f;
        if (attacker != null)
        {
            HunterMarkBuff mark = attacker.GetComponent<HunterMarkBuff>();
            if (mark != null)
            {
                damageMultiplier = mark.GetDamageMultiplier();
            }
        }

        int finalDamage = Mathf.FloorToInt(damage * damageMultiplier * (isLightDamage ? lightVulnerabilityMultiplier : 1f));
        health.TakeDamage(finalDamage, attacker);
    
        // При получении урона возвращаемся в зону патрулирования
        if (isAttachedToPlayer)
        {
            DetachFromPlayer();
            ReturnToPatrolZone();
        }
    
        // Прерываем stealth при получении урона
        if (isInStealth)
        {
            if (stealthCoroutine != null)
                StopCoroutine(stealthCoroutine);
        
            isInStealth = false;
            RpcSetStealthVisuals(false);
            StartCoroutine(DelayedStealthRestart());
        }
    }
    [Server]
    private void ReturnToPatrolZone()
    {
        // Останавливаем все текущие действия
        if (attachmentCoroutine != null)
        {
            StopCoroutine(attachmentCoroutine);
            attachmentCoroutine = null;
        }
    
        if (debuffCoroutine != null)
        {
            StopCoroutine(debuffCoroutine);
            RemoveDebuff();
        }
    
        // Возвращаемся к центру зоны патрулирования
        currentTarget = null;
        isAttachedToPlayer = false;
        isPouncing = false;
        isStunned = false;
    
        // Запускаем корутину возврата
        StartCoroutine(ReturnToPatrolRoutine());
    }
    
    [Server]
    private IEnumerator ReturnToPatrolRoutine()
    {
        // Двигаемся к центру зоны патрулирования
        while (Vector2.Distance(transform.position, patrolCenter) > patrolPointReachedThreshold)
        {
            Vector2 direction = (patrolCenter - (Vector2)transform.position).normalized;
            rb.linearVelocity = direction * health.CurrentAttack * 0.5f;
            RpcUpdateAnimator(direction, rb.linearVelocity.magnitude);
            yield return null;
        }
    
        // Останавливаемся и начинаем патрулирование
        rb.linearVelocity = Vector2.zero;
        RpcUpdateAnimator(Vector2.zero, 0f);
    
        StartPatrol();
        StartStealthCycle();
    }
    
    [Server]
    private IEnumerator DelayedStealthRestart()
    {
        yield return new WaitForSeconds(2f);
        StartStealthCycle();
    }

    [Server]
    private void OnEnemyDeath()
    {
        if (isAttachedToPlayer && debuffedPlayer != null)
        {
            debuffedPlayer.UnregisterAttachedEnemy();
        }
    
        // Остальной код остается без изменений
        if (stealthCoroutine != null)
            StopCoroutine(stealthCoroutine);
    
        if (patrolCoroutine != null)
            StopCoroutine(patrolCoroutine);
        
        if (debuffCoroutine != null)
        {
            StopCoroutine(debuffCoroutine);
            RemoveDebuff();
        }
    
        if (attachmentCoroutine != null)
        {
            StopCoroutine(attachmentCoroutine);
            isAttachedToPlayer = false;
        }
    }

    [Server]
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer || !isPouncing) return;
        
        PlayerStats player = collision.gameObject.GetComponent<PlayerStats>();
        if (player != null)
        {
            player.TakeHit(health.CurrentAttack);
            
            // Apply root effect to player
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.ApplyRoot(0.5f);
            }
            
            // Начинаем накладывать дебафф при столкновении
            debuffedPlayer = player;
            if (debuffCoroutine != null)
                StopCoroutine(debuffCoroutine);
                
            debuffCoroutine = StartCoroutine(ApplyDebuffRoutine());
        }
    }



    // Визуализация зоны патрулирования в редакторе
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(patrolCenter, patrolRadius);
            
            if (isPatrolling)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, currentPatrolPoint);
                Gizmos.DrawSphere(currentPatrolPoint, 0.3f);
            }
        }
    }
}