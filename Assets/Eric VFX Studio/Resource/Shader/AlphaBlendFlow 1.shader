Shader "UI/SmoothOutlineHDR"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(0,10)) = 2
        _OutlineSoftness ("Outline Softness", Range(0.1,5)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _OutlineSoftness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _OutlineSize;

                float centerAlpha = tex2D(_MainTex, i.uv).a;

                float maxAlpha = 0;

                for (int x = -2; x <= 2; x++)
                {
                    for (int y = -2; y <= 2; y++)
                    {
                        float2 offset = float2(x,y) * texel * 0.5;
                        maxAlpha = max(maxAlpha, tex2D(_MainTex, i.uv + offset).a);
                    }
                }

                float outline = smoothstep(0.0, _OutlineSoftness, maxAlpha - centerAlpha);

                fixed4 baseColor = tex2D(_MainTex, i.uv) * i.color;

                fixed4 finalColor = baseColor;

                finalColor.rgb = lerp(_OutlineColor.rgb, baseColor.rgb, centerAlpha);
                finalColor.a = max(baseColor.a, outline * _OutlineColor.a);

                return finalColor;
            }
            ENDCG
        }
    }
}