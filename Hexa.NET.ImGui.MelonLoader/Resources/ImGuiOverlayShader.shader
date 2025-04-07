Shader "Custom/ImGuiOverlayShader"
{
    Properties
    {
        _FontTexture ("Texture", 2D) = "white" {} 
    }
    SubShader
    {
        Tags { "Queue"="Overlay" }
        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FontTexture;
            float4x4 _ProjectionMatrix;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(_ProjectionMatrix, float4(v.vertex.xy, 0.f, 1.f));
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                return tex2D(_FontTexture, i.uv) * i.color; 
            }
            ENDCG
        }
    }
}
