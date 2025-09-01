Shader "UI/CircleCutout_RectAspect"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color   ("Overlay Color", Color) = (0,0,0,1)
        _Radius  ("Radius (0-0.5)", Range(0,0.6)) = 0.35
        _Feather ("Edge Feather", Range(0,0.2)) = 0.02
        _Center  ("Center (UV 0..1)", Vector) = (0.5, 0.5, 0, 0)
        _RectAspect ("Rect Aspect (w/h)", Float) = 1.0

        // Stencil (để Mask/RectMask2D hoạt động như UI/Default)
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID", Float) = 0
        _StencilOp        ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask",  Float) = 255
        _ColorMask        ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref      [_Stencil]
            Comp     [_StencilComp]
            Pass     [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _Color;
            float  _Radius;
            float  _Feather;
            float4 _Center;      // xy = uv center (0..1 trong Rect)
            float  _RectAspect;  // = rect.width / rect.height

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = TRANSFORM_TEX(v.uv, _MainTex);
                o.color  = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // uv 0..1 trên chính Rect (khuyên dùng RawImage "Simple", Native Size / fill full rect)
                float2 d = i.uv - _Center.xy;

                // Giữ hình tròn theo tỉ lệ RECT (không dùng screen aspect để tránh méo khi rect không full-screen)
                d.x *= _RectAspect;

                float dist = length(d);

                float edgeStart = max(_Radius - _Feather, 0);
                // mask = 1 ở ngoài hình tròn (đổ màu), 0 ở trong (trong suốt)
                float mask = smoothstep(edgeStart, _Radius, dist);

                fixed4 col = _Color * i.color; // tôn trọng alpha của UI Graphic
                col.a *= mask;
                return col;
            }
            ENDCG
        }
    }
}
