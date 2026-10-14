Shader "Andam/QuietWater"
{
    Properties { _Color ("Muted water",Color)=(0.29,0.58,0.64,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; UNITY_FOG_COORDS(1) };
            v2f vert(float4 p:POSITION) { v2f o; o.pos=UnityObjectToClipPos(p); o.world=mul(unity_ObjectToWorld,p).xyz; UNITY_TRANSFER_FOG(o,o.pos); return o; }
            fixed4 frag(v2f i):SV_Target
            {
                half wash=sin(i.world.x*.18+i.world.z*.12+_Time.y*.35)*.022;
                fixed4 c=fixed4(_Color.rgb+wash,1); UNITY_APPLY_FOG(i.fogCoord,c); return c;
            }
            ENDCG
        }
    }
}
