using UnityEngine;
using Mirror;
using System.Collections;

public class SpeedBoostBuff : NetworkBehaviour
{
    [SyncVar] private float duration;
    [SyncVar] private float speedMultiplier;
    
    private PlayerMovement playerMovement;
    private float originalSpeed;

    public void SetParameters(float newDuration, float newMultiplier)
    {
        duration = newDuration;
        speedMultiplier = newMultiplier;
    }

    public void ApplyBuff()
    {
        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
            originalSpeed = playerMovement.moveSpeed;
        }
        
        playerMovement.moveSpeed = originalSpeed * speedMultiplier;
        RpcShowSpeedBoostEffect(true);
        
        StartCoroutine(RemoveAfterDuration());
    }

    private IEnumerator RemoveAfterDuration()
    {
        yield return new WaitForSeconds(duration);
        RemoveEffect();
        Destroy(this);
    }

    private void RemoveEffect()
    {
        if (playerMovement != null)
        {
            playerMovement.moveSpeed = originalSpeed;
        }
        RpcShowSpeedBoostEffect(false);
    }

    [ClientRpc]
    private void RpcShowSpeedBoostEffect(bool show)
    {
        // Визуальный эффект ускорения (например, следы за игроком)
    }

    private void OnDestroy()
    {
        RemoveEffect();
    }
}