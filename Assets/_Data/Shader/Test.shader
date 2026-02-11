Shader "Custom/SpriteDirectionalReveal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Reveal ("Reveal", Range(0,1)) = 0
        _Direction ("Direction (0=L2R,1=R2L,2=B2T,3=T2B)", Float) = 2
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _Reveal;
            float _Direction;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float value;

                if (_Direction == 0)       value = i.uv.x;          // Left → Right
                else if (_Direction == 1)  value = 1 - i.uv.x;      // Right → Left
                else if (_Direction == 2)  value = i.uv.y;          // Bottom → Top
                else                       value = 1 - i.uv.y;      // Top → Bottom

                if (value > _Reveal)
                    discard;

                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
