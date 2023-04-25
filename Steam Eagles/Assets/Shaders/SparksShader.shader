Shader "Unlit/SparksShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        [HDR] _HDRColor ("HDR Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {  
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "UniversalMaterialType" = "Unlit"
            "Queue"="Transparent"
            }
        LOD 100

        Pass
        {
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
             fixed4 _HDRColor;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                const float alpha = col.a * i.color.a;
                col *= i.color * _HDRColor;
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                col.a = alpha;
                return col;
            }
            ENDCG
        }
    }
}
