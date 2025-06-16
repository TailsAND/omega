// using UnityEngine;
// using Mirror;
// using System.Collections;
//
// public class RegenerationBuff : NetworkBehaviour
// {
//     [SyncVar] private float duration;
//     [SyncVar] private float healPerTick;
//     [SyncVar] private float tickInterval;
//     
//     private PlayerStats playerStats;
//     private Coroutine regenerationCoroutine;
//
//     public void SetParameters(float newDuration, float newHeal, float newInterval)
//     {
//         duration = newDuration;
//         healPerTick = newHeal;
//         tickInterval = newInterval;
//     }
//
//     public void ApplyBuff()
//     {
//         if (playerStats == null)
//         {
//             playerStats = GetComponent<PlayerStats>();
//         }
//         
//         if (regenerationCoroutine != null)
//             StopCoroutine(regenerationCoroutine);
//             
//         regenerationCoroutine = StartCoroutine(RegenerationEffect());
//         RpcShowRegenerationEffect(true);
//     }
//
//     private IEnumerator RegenerationEffect()
//     {
//         float endTime = Time.time + duration;
//         
//         while (Time.time < endTime && playerStats != null)
//         {
//             playerStats.Heal((int)healPerTick);
//             yield return new WaitForSeconds(tickInterval);
//         }
//         
//         RpcShowRegenerationEffect(false);
//         Destroy(this);
//     }
//
//     [ClientRpc]
//     private void RpcShowRegenerationEffect(bool show)
//     {
//         // Визуальный эффект регенерации (например, золотые частицы)
//     }
// }