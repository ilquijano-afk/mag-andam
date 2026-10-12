Shader "Andam/OpongPainted"
{
    Properties { _MainTex ("1024 painted palette atlas",2D)="white" {} }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150
        CGPROGRAM
        #pragma surface surf Opong fullforwardshadows addshadow noforwardadd noambient
        #pragma target 3.0
        #include "Lighting.cginc"
        sampler2D _MainTex;
        struct Input { float2 uv_MainTex; };
        half4 LightingOpong(SurfaceOutput s,half3 lightDir,half3 viewDir,half atten)
        {
            half wrap=smoothstep(-0.22,0.92,dot(s.Normal,lightDir));
            half3 light=lerp(half3(.83,.86,.94),_LightColor0.rgb,wrap)*(.70+.30*wrap)*(.82+.18*atten);
            half highlight=pow(saturate(dot(s.Normal,normalize(lightDir+viewDir))),22)*s.Specular*atten;
            half rim=pow(1-saturate(dot(s.Normal,viewDir)),4)*.035;
            return half4(s.Albedo*(light+rim)+highlight*half3(.85,.91,.97),1);
        }
        void surf(Input IN,inout SurfaceOutput o)
        {
            fixed4 paint=tex2D(_MainTex,IN.uv_MainTex); o.Albedo=paint.rgb; o.Specular=paint.a; o.Alpha=1;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
