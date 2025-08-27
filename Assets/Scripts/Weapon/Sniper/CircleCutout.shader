Shader "UI/CircleCutout"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Overlay Color", Color) = (0,0,0,1)
        _Radius ("Radius (0-0.5 roughly)", Range(0,0.6)) = 0.35
        _Feather ("Edge Feather", Range(0,0.2)) = 0.02
        _Center ("Center (UV)", Vector) = (0.5, 0.5, 0, 0)
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
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Radius;
            float _Feather;
            float4 _Center; // xy = uv center

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // UV in [0,1], center to _Center.xy
                float2 uv = i.uv;
                float2 c = _Center.xy;
                float2 d = uv - c;

                // keep circle independent of aspect ratio
                float aspect = _ScreenParams.x / _ScreenParams.y;
                d.x *= aspect;

                float dist = length(d);
                // mask: 1 outside circle, 0 inside (with feather)
                float edgeStart = max(_Radius - _Feather, 0);
                float mask = smoothstep(edgeStart, _Radius, dist);

                fixed4 col = _Color;
                col.a *= mask; // fully transparent inside the circle
                return col;
            }
            ENDCG
        }
    }
}
