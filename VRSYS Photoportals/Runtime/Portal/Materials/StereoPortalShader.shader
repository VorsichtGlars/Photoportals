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

/**
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
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO //Insert
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
                UNITY_SETUP_INSTANCE_ID(input); //Insert
                ZERO_INITIALIZE(Varyings, output); //Insert
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output); //Insert

                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                //UNITY_SETUP_STEREO_EYE_INDEX;
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input); //Insert
                float3 color = unity_StereoEyeIndex == 0 
                    ? SAMPLE_TEXTURE2D(_LeftEyeTexture, sampler_LeftEyeTexture, input.uv).rgb 
                    : SAMPLE_TEXTURE2D(_RightEyeTexture, sampler_RightEyeTexture, input.uv).rgb;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
}
    **/

Shader "Custom/StereoPortalShader"
{
    Properties
    {
        _MainTex("Mono Texture (unused)", 2D) = "white" {}
        [PerRendererData] _LeftEyeTexture("Left Eye Texture", 2D) = "white" {}
        [PerRendererData] _RightEyeTexture("Right Eye Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
            TEXTURE2D(_LeftEyeTexture); SAMPLER(sampler_LeftEyeTexture);
            TEXTURE2D(_RightEyeTexture);SAMPLER(sampler_RightEyeTexture);

            Varyings vert(Attributes input)
            {
                Varyings output;
                ZERO_INITIALIZE(Varyings, output);
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                /**
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return (unity_StereoEyeIndex == 0) ? half4(1,0,0,1) : half4(0,0,1,1);
                **/
                
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 color = (unity_StereoEyeIndex == 0)
                    ? SAMPLE_TEXTURE2D(_LeftEyeTexture,  sampler_LeftEyeTexture,  input.uv).rgb
                    : SAMPLE_TEXTURE2D(_RightEyeTexture, sampler_RightEyeTexture, input.uv).rgb;

                return half4(color, 1);
                
            }
            ENDHLSL
        }
    }
}