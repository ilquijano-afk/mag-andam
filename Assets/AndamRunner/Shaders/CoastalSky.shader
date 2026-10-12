Shader "Andam/CoastalSky"
{
    Properties { _Top ("Sky", Color) = (0.36,0.73,0.89,1) _Horizon ("Horizon", Color) = (1,0.84,0.67,1) }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Top, _Horizon;
            struct v2f { float4 position : SV_POSITION; float3 direction : TEXCOORD0; };
            v2f vert(float4 vertex : POSITION) { v2f o; o.position=UnityObjectToClipPos(vertex); o.direction=vertex.xyz; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                float3 direction=normalize(i.direction);
                float height=saturate(direction.y*1.2+0.05);
                float3 color=lerp(_Horizon.rgb,_Top.rgb,smoothstep(0,1,height));
                float sun=pow(saturate(dot(direction,normalize(float3(0.45,0.38,0.80)))),350);
                color=lerp(color,float3(1,0.97,0.76),sun);
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
