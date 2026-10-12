Shader "Andam/CoastalToon"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Compatibility", Range(0,1)) = 0
        _Sheen ("Soft painted highlight", Range(0,1)) = 0.025
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        CGPROGRAM
        #pragma surface surf Coastal fullforwardshadows addshadow noforwardadd noambient
        #pragma target 3.0
        #include "Lighting.cginc"
        fixed4 _Color;
        half _Sheen;
        struct Input { float3 viewDir; float3 worldPos; };
        half4 LightingCoastal(SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
        {
            half diffuse = saturate(dot(s.Normal, lightDir));
            half ramp = smoothstep(0.02, 0.95, diffuse);
            half rim = pow(1.0 - saturate(dot(s.Normal, viewDir)), 3.0) * 0.045;
            half3 light = lerp(half3(0.87,0.90,0.98),_LightColor0.rgb,ramp) * (0.80 + ramp * 0.20) * (0.83 + atten * 0.17);
            half sheen=pow(saturate(dot(s.Normal,normalize(lightDir+viewDir))),14.0)*_Sheen*atten;
            return half4(s.Albedo * (light + rim) + sheen * half3(0.85,0.91,0.97), s.Alpha);
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            // Quiet pigment variation, anchored to object space so moving scenery does not shimmer.
            float3 p=mul(unity_WorldToObject,float4(IN.worldPos,1)).xyz;
            float grain=frac(sin(dot(floor(p*180),float3(12.9898,78.233,37.719)))*43758.5453);
            float wash=sin(p.x*14+p.y*8)*sin(p.z*11+p.y*5);
            float3 footprint=fwidth(p*180);
            float visibleGrain=1-saturate(max(footprint.x,max(footprint.y,footprint.z)));
            o.Albedo = _Color.rgb * (0.994 + (grain-.5) * 0.025 * visibleGrain + wash * 0.014);
            o.Alpha = 1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
