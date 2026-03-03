Shader "UI/OutlineHDR"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Float) = 1
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
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;

            fixed4 _OutlineColor;
            float _OutlineSize;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                if (col.a > 0.01)
                    return col;

                float2 offset = _MainTex_TexelSize.xy * _OutlineSize;

                float alpha = 0;

                alpha += tex2D(_MainTex, i.texcoord + float2(offset.x, 0)).a;
                alpha += tex2D(_MainTex, i.texcoord + float2(-offset.x, 0)).a;
                alpha += tex2D(_MainTex, i.texcoord + float2(0, offset.y)).a;
                alpha += tex2D(_MainTex, i.texcoord + float2(0, -offset.y)).a;

                alpha += tex2D(_MainTex, i.texcoord + offset).a;
                alpha += tex2D(_MainTex, i.texcoord - offset).a;
                alpha += tex2D(_MainTex, i.texcoord + float2(offset.x, -offset.y)).a;
                alpha += tex2D(_MainTex, i.texcoord + float2(-offset.x, offset.y)).a;

                if (alpha > 0)
                    return _OutlineColor;

                return 0;
            }
            ENDCG
        }
    }
}