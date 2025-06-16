using UnityEngine;
using Mirror;

[RequireComponent(typeof(PlayerStats))]
public class PlayerEdgeEffects : NetworkBehaviour
{
    [Header("Material")]
    public Material edgeEffectMaterial;
    private Material runtimeMaterial;

    [Header("Settings")]
    public float effectDuration = 1f;
    public float maxIntensity = 0.8f;
    public Color damageColor = Color.red;

    private void Start()
    {
        // Создаем runtime-копию материала
        if (isLocalPlayer && Input.GetKeyDown(KeyCode.T))
        {
            RpcActivateEffect();
        }
    }

    [ClientRpc]
    public void RpcActivateEffect()
    {
        if (!isLocalPlayer || runtimeMaterial == null) return;
        
        StopAllCoroutines();
        StartCoroutine(PlayEffect());
    }

    private System.Collections.IEnumerator PlayEffect()
    {
        float timer = 0;
        
        while (timer < effectDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.PingPong(timer / effectDuration * 2, 1);
            runtimeMaterial.SetFloat("_Intensity", progress * maxIntensity);
            yield return null;
        }
        
        runtimeMaterial.SetFloat("_Intensity", 0);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Debug.Log($"Rendering: mat={(runtimeMaterial!=null)}");
        Graphics.Blit(src, dest, runtimeMaterial);
    }
}