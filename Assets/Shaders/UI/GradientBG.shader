Shader "Snowy/GradientBG"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0.5, 0.5, 0.5, 1.0)
        _Color2 ("Color 2", Color) = (1.0, 1.0, 1.0, 1.0)
        _Angle ("Angle (DEGREES)", Float) = 0.0
        
        // Noise
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 1.0
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

            float4 _Color1, _Color2;
            float _Angle;
            float _NoiseScale;

            sampler2D _NoiseTex;

            float2 Rotate(float2 uv, float2 center, float rot)
            {
                float2 offset = uv - center;
                float s = sin(rot);
                float c = cos(rot);
                float2 newUV;
                newUV.x = offset.x * c - offset.y * s;
                newUV.y = offset.x * s + offset.y * c;
                return newUV + center;
            }

            float Noise(float2 uv)
            {
                return tex2D(_NoiseTex, uv).r;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate the gradient
                float angle = _Angle * 3.14159265358979323846 / 180.0;

                // Center is the middle of the screen
                float2 center = float2(0.5, 0.5);

                float2 uv = Rotate(i.uv, center, angle);

                float gradient = uv.x;
                
                // Interpolate between the two colors based on the gradient

                // Add custom noise at the gradient line
                float4 color = lerp(_Color1, (0, 0, 0, 0), gradient);


                // Add Noise around the gradient line
                float4 second = lerp((0, 0, 0, 0), _Color2, gradient);
                if (gradient > 0.4 && gradient < 0.6)
                {
                    // Smudge effects
                    float noise = Noise(i.uv * _NoiseScale);

                    // Radial gradient smudge
                    float2 radial = i.uv - center;
                    float radialDist = length(radial);
                    float radialGradient = smoothstep(0.5, 0.6, radialDist);
                    float4 radialColor = lerp(_Color1, _Color2, radialGradient);

                    // Add radial color to the gradient
                    second = lerp(second, radialColor, noise);
                }
                
                // Add color2 to it faded

                // Add both colors
                color += second;

                return color;
            }
            
            ENDCG
        }
    }
}
