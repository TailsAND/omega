Shader "Custom/OcclusionMaskShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ZTest LEqual

        Pass
        {
            Cull Back
            Offset 1, 1
            SetTexture [_MainTex]
            {
                combine primary
            }
        }
    }
    FallBack "Diffuse"
}
