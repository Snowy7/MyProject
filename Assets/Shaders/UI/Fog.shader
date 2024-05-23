Shader "Hidden/Fog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        
        // Layer 1
        _FogColor_1 ("Fog Color 1", Color) = (0, 0, 0, 1)
        _FogSpeed_1 ("Fog Speed 1", Vector) = (0, 0, 0, 0)
        _FogSize_1 ("Fog Size 1", Range(0, 1)) = 0.5
        
        // Layer 2
        _FogColor_2 ("Fog Color 2", Color) = (0, 0, 0, 1)
        _FogSpeed_2 ("Fog Speed 2", Vector) = (0, 0, 0, 0)
        _FogSize_2 ("Fog Size 2", Range(0, 1)) = 0.5
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/Shaders/Includes/Noise.hlsl"

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
            

            float4 GetFogColor(float4 fogColor, float4 fogSpeed, float fogSize)
            {
                float2 uv = float2(_Time.y * fogSpeed.x, _Time.y * fogSpeed.y);
                float noise = SimplexNoise(uv) * fogSize;
                float4 color = noise * fogColor;
                return color;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;

            fixed4 _FogColor_1;
            float4 _FogSpeed_1;
            float _FogSize_1;
            
            fixed4 _FogColor_2;
            float4 _FogSpeed_2;
            float _FogSize_2;

            fixed4 frag (v2f i) : SV_Target
            {

                float4 fogColor1 = GetFogColor(_FogColor_1, _FogSpeed_1, _FogSize_1);
                float4 fogColor2 = GetFogColor(_FogColor_2, _FogSpeed_2, _FogSize_2);

                float4 col = fogColor1 * fogColor2;
                
                return col;
            }
            ENDCG
        }
    }
}
