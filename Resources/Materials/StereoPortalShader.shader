/**
Shader "Custom/StereoPortalShader"
{
    Properties{
      _MainTex("Mono Texture (unused)", 2D) = "white" {}
      _LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
      _RightEyeTexture("Right Eye Texture", 2D) = "white" {}
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }

        CGPROGRAM

        #pragma surface surf Lambert

        struct Input {
            float2 uv_MainTex;
        };

        sampler2D _MainTex;
        sampler2D _LeftEyeTexture;
        sampler2D _RightEyeTexture;
        
        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = unity_StereoEyeIndex == 0 ? tex2D(_LeftEyeTexture, IN.uv_MainTex).rgb : tex2D(_RightEyeTexture, IN.uv_MainTex).rgb;
        }

        ENDCG
    }
    Fallback "Diffuse"
}
**/

Shader "Custom/StereoPortalShader"
{
    Properties
    {
        _MainTex("Mono Texture (unused)", 2D) = "white" {}
        _LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
        _RightEyeTexture("Right Eye Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_LeftEyeTexture);
            SAMPLER(sampler_LeftEyeTexture);

            TEXTURE2D(_RightEyeTexture);
            SAMPLER(sampler_RightEyeTexture);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                //UNITY_SETUP_STEREO_EYE_INDEX
                float3 color = unity_StereoEyeIndex == 0 
                    ? SAMPLE_TEXTURE2D(_LeftEyeTexture, sampler_LeftEyeTexture, input.uv).rgb 
                    : SAMPLE_TEXTURE2D(_RightEyeTexture, sampler_RightEyeTexture, input.uv).rgb;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
