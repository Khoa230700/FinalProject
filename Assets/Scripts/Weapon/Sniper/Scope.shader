Shader "Custom/CircleScopeMask"
{
    Properties
    {
        _MainTex ("Scope Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Radius ("Scope Radius", Range(0.1, 0.6)) = 0.45
        _EdgeSmooth ("Edge Smoothness", Range(0.001, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _EdgeSmooth;

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
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float2 center = float2(0.5, 0.5);
                float dist = distance(uv, center);

                float mask = smoothstep(_Radius, _Radius - _EdgeSmooth, dist);
                fixed4 col = tex2D(_MainTex, uv) * _Color;
                col.a *= mask; // mask alpha

                return col;
            }
            ENDCG
        }
    }
}
