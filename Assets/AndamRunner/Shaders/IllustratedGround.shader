Shader "Andam/IllustratedGround"
{
    Properties { _Color ("Lane tint", Color) = (0.65,0.66,0.65,0.66) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        CGPROGRAM
        #pragma surface surf Ground fullforwardshadows alpha:fade noforwardadd noambient
        #pragma target 3.0
        fixed4 _Color;
        struct Input { float3 worldPos; };
        half4 LightingGround(SurfaceOutput s,half3 lightDir,half atten)
        { return half4(s.Albedo*(0.95+0.05*saturate(dot(s.Normal,lightDir)))*(0.86+0.14*atten),s.Alpha); }
        void surf(Input IN,inout SurfaceOutput o)
        {
            o.Albedo=_Color.rgb;
            o.Alpha=_Color.a*(1-smoothstep(25,62,IN.worldPos.z));
        }
        ENDCG
    }
    Fallback "Transparent/Diffuse"
}
