using UnityEngine;

public class PlayerAttack : MonoBehaviour 
{
    [Header("Attack Settings")] 
    public int baseDamage = 10;
    public float attackRange = 1f;
    public float attackCooldown = 0.5f;
    public Vector2 attackOffset = new Vector2(0.5f, 0);
    
    [Header("References")]
    public LayerMask enemyLayer;
    public Animator animator;
    public AudioClip attackSound;
    public PlayerStats playerStats;
    
    private float lastAttackTime;
    private bool isAttacking;
    private int calculatedDamage;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
       
        Debug.Log("Attack initiated");
        calculatedDamage = baseDamage + Mathf.FloorToInt(3 * 0.5f);
    
        Vector2 attackPos = (Vector2)transform.position + 
                            attackOffset * (transform.localScale.x > 0 ? 1 : -1);

        Debug.DrawRay(attackPos, Vector2.right * attackRange, Color.red, 1f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPos,
            attackRange,
            enemyLayer
        );
        Debug.Log($"Found {hitEnemies.Length} enemies in attack range");

        bool hitConnected = false;
        
        foreach (Collider2D enemy in hitEnemies)
        {
            TestenemyHealthSimp enemyHealth = enemy.GetComponent<TestenemyHealthSimp>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(calculatedDamage, playerStats);
                Debug.Log($"Hit {enemy.name} for {calculatedDamage} damage");
                hitConnected = true;
            }
        }

        if (hitConnected)
        {
            PlayAttackEffects();
        }

        lastAttackTime = Time.time;
    }

    void PlayAttackEffects()
    {

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (attackSound != null)
        {
            AudioSource.PlayClipAtPoint(attackSound, transform.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector2 attackPos = (Vector2)transform.position +
                          attackOffset * (transform.localScale.x > 0 ? 1 : -1);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);
    }


    public int CurrentDamage => calculatedDamage;
}

