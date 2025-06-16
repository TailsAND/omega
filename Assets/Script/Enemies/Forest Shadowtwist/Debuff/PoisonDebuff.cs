using UnityEngine;
using Mirror;
using System.Collections;

public class PoisonDebuff : NetworkBehaviour
{
    [SyncVar] private float duration;
    [SyncVar] private float damagePerTick;
    [SyncVar] private float tickInterval;
    [SyncVar] private float slowPercentage;
    
    private PlayerStats playerStats;
    private PlayerMovement playerMovement;
    private Coroutine poisonCoroutine;
    private float originalSpeed;

    public void SetParameters(float newDuration, float newDamage, float newInterval, float newSlowPercentage)
    {
        duration = newDuration;
        damagePerTick = newDamage;
        tickInterval = newInterval;
        slowPercentage = newSlowPercentage;
    }

    public void ApplyDebuff()
    {
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
            playerMovement = GetComponent<PlayerMovement>();
            originalSpeed = playerMovement.moveSpeed;
        }
        
        // Применяем замедление
        playerMovement.moveSpeed = originalSpeed * (1 - slowPercentage);
        
        if (poisonCoroutine != null)
            StopCoroutine(poisonCoroutine);
            
        poisonCoroutine = StartCoroutine(PoisonEffect());
        RpcShowPoisonEffect(true);
    }

    private IEnumerator PoisonEffect()
    {
        float endTime = Time.time + duration;
        
        while (Time.time < endTime && playerStats != null)
        {
            playerStats.TakeHit((int)damagePerTick);
            yield return new WaitForSeconds(tickInterval);
        }
        
        RemoveEffect();
        Destroy(this);
    }

    private void RemoveEffect()
    {
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalSpeed;
        }
        RpcShowPoisonEffect(false);
    }

    [ClientRpc]
    private void RpcShowPoisonEffect(bool show)
    {
        // Визуальный эффект отравления (например, зеленые частицы)
    }

    private void OnDestroy()
    {
        RemoveEffect();
    }
}