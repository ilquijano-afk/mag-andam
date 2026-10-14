Shader "Andam/QuietRain"
{
    Properties { _Amount ("Rain amount",Range(0,1))=0 _Color("Rain",Color)=(0.83,0.90,0.94,0.22) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color; half _Amount;
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct v2f { float4 pos:SV_POSITION; };
            v2f vert(appdata v) { v2f o; v.vertex.y-=frac(_Time.y*.22+v.uv.x)*32; o.pos=UnityObjectToClipPos(v.vertex); return o; }
            fixed4 frag(v2f i):SV_Target { return fixed4(_Color.rgb,_Color.a*_Amount); }
            ENDCG
        }
    }
}
