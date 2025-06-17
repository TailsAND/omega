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
    public Color poisonColor = Color.green;
    public Color fearColor = Color.magenta;

    private PlayerStats playerStats;

    private void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        runtimeMaterial = new Material(edgeEffectMaterial);
    }

    [ClientRpc]
    public void RpcActivateEffect(Color effectColor)
    {
        if (!isLocalPlayer || runtimeMaterial == null) return;
        
        runtimeMaterial.SetColor("_EdgeColor", effectColor);
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
        if (runtimeMaterial != null)
        {
            Graphics.Blit(src, dest, runtimeMaterial);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }

    // Вызывается при получении урона
    public void OnDamageTaken()
    {
        if (isLocalPlayer)
        {
            RpcActivateEffect(damageColor);
        }
    }
    public void OnEffectEnd()
    {
        if (!isLocalPlayer || runtimeMaterial == null) return;
        runtimeMaterial.SetFloat("_Intensity", 0);
    }
    
    // Вызывается при отравлении
    public void OnPoisonEffect()
    {
        if (isLocalPlayer)
        {
            RpcActivateEffect(poisonColor);
        }
    }

    // Вызывается при страхе
    public void OnFearEffect()
    {
        if (isLocalPlayer)
        {
            RpcActivateEffect(fearColor);
        }
    }
}