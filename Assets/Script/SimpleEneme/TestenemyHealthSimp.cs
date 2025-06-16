using UnityEngine;
using Mirror;

public class TestenemyHealthSimp : NetworkBehaviour
{
    private EnemyControllerSimp enemyController;

    private void Start()
    {
        enemyController = GetComponent<EnemyControllerSimp>();
    }

    public void TakeDamage(int damage, PlayerStats attacker)
    {
        Debug.Log($"Taking damage: {damage}");
        if (enemyController != null)
        {
            enemyController.TakeDamage(damage, attacker);
        }
        else
        {
            Debug.LogError("EnemyControllerSimp component is missing!");
        }
    }
}