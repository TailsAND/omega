using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class EnemyControllerSimp : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private float waitTimeAtPoint = 1f;
    
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    [SyncVar(hook = nameof(OnHealthChanged))] 
    private int currentHealth;
    
    [Header("HP Display")]
    [SerializeField] private Image hpFillImage; // Ссылка на UI Image
    
    private int currentPatrolIndex = 0;
    private float waitCounter;
    private bool isWaiting = false;
    private SpriteRenderer spriteRenderer;
    
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;
        
        if (hpFillImage != null)
        {
            hpFillImage.type = Image.Type.Filled;
            hpFillImage.fillMethod = Image.FillMethod.Horizontal;
            hpFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            UpdateHpDisplay();
        }
    }
    
    private void Update()
    {
        if (!isServer) return;
        
        Patrol();
    }
    
    private void Patrol()
    {
        if (isWaiting)
        {
            waitCounter -= Time.deltaTime;
            if (waitCounter <= 0f)
            {
                isWaiting = false;
                currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            }
            return;
        }
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;
        
        transform.position = Vector2.MoveTowards(
            transform.position, 
            targetPoint.position, 
            moveSpeed * Time.deltaTime
        );
        
        if (direction.x > 0) spriteRenderer.flipX = false;
        else if (direction.x < 0) spriteRenderer.flipX = true;
        
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            isWaiting = true;
            waitCounter = waitTimeAtPoint;
        }
    }
    
    // Добавьте этот метод в EnemyControllerSimp
    [Server]
    public void TakeDamage(int damage, PlayerStats attacker)
    {
        currentHealth -= damage;
    
        if (currentHealth <= 0)
        {
            Die();
        }
    
        // Синхронизация здоровья
        OnHealthChanged(currentHealth, currentHealth);
    
        // Можно добавить эффект получения урона
        RpcPlayDamageEffect();
    }

    [ClientRpc]
    private void RpcPlayDamageEffect()
    {
        StartCoroutine(FlashRed());
    }
    
    private void Die()
    {
        NetworkServer.Destroy(gameObject);
    }
    
    private void OnHealthChanged(int oldHealth, int newHealth)
    {
        UpdateHpDisplay();
        StartCoroutine(FlashRed());
    }
    
    
    
    private void UpdateHpDisplay()
    {
        if (hpFillImage != null)
        {
            float fillAmount = (float)currentHealth / maxHealth;
            hpFillImage.fillAmount = fillAmount;
            
            // Изменение цвета от зеленого к красному
            hpFillImage.color = Color.Lerp(Color.red, Color.green, fillAmount);
        }
    }
    
    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = Color.white;
    }
    
    private void OnDrawGizmos()
    {
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < patrolPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i+1].position);
            }
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length-1].position, patrolPoints[0].position);
        }
    }
}