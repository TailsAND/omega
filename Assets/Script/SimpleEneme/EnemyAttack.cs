using UnityEngine;
using Mirror;

public class EnemyAttack : NetworkBehaviour
{
    public int damage = 10; // Урон за атаку
    public float attackDelay = 1f; // Задержка между атаками
    
    private float lastAttackTime;
    private PlayerStats playerStats;

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isServer) return;
        
        // Проверяем, что это игрок и можно атаковать
        if (Time.time - lastAttackTime >= attackDelay)
        {
            playerStats = collision.gameObject.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                AttackPlayer();
                lastAttackTime = Time.time;
            }
        }
    }

    [Server]
    private void AttackPlayer()
    {
        playerStats.TakeHit(damage);
    }
}