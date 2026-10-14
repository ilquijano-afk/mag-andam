Shader "Andam/EnvironmentPalette"
{
    Properties { _Tint ("Weather tint", Color) = (1,1,1,1) _Shade ("Soft shade", Range(0,1)) = 0.18 }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            fixed4 _Tint;
            half _Shade;
            struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; fixed4 color:COLOR; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct v2f { float4 pos:SV_POSITION; fixed4 color:COLOR; UNITY_FOG_COORDS(0) };
            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v); v2f o; o.pos=UnityObjectToClipPos(v.vertex);
                half diffuse=saturate(dot(UnityObjectToWorldNormal(v.normal),normalize(_WorldSpaceLightPos0.xyz)));
                half shade=lerp(1-_Shade,1,smoothstep(0,1,diffuse));
                o.color=fixed4(v.color.rgb*_Tint.rgb*shade,1);
                UNITY_TRANSFER_FOG(o,o.pos); return o;
            }
            fixed4 frag(v2f i):SV_Target { fixed4 c=i.color; UNITY_APPLY_FOG(i.fogCoord,c); return c; }
            ENDCG
        }
    }
}
