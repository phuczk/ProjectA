Shader "Unlit/one-side"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CullMode)] _Cull("Cull", Float)=0
    }
    SubShader
    {
        // 1. Phải có Queue Transparent để vẽ sau các vật thể đục
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        
        Lighting Off 
        ZWrite Off
        
        // 2. LỆNH QUAN TRỌNG: Cho phép trộn màu dựa trên kênh Alpha của texture
        Blend SrcAlpha OneMinusSrcAlpha 
        
        Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // col.a chính là kênh Alpha của PNG, Blend sẽ dùng nó để cắt bỏ nền đen
                return col;
            }
            ENDCG
        }
    }
}