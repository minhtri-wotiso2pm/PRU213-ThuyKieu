// Polished-metal mirror for PlanarMirror: shows the reflection camera's image, looked up in screen space so the
// reflection lines up with the scene from any viewpoint, tinted like bronze and slightly hazy towards the rim.
//   _ReflectionTex  set at runtime by PlanarMirror (mirrored horizontally: see PlanarMirror)
//   _Haze           how milky the metal is (old bronze is never a perfect mirror)
Shader "ThuyKieu/StoryMirror"
{
    Properties
    {
        [MainColor] _BaseColor("Metal tint", Color) = (1, 0.86, 0.62, 1)
        _HazeColor("Haze colour", Color) = (0.55, 0.42, 0.25, 1)
        _Haze("Haze", Range(0, 1)) = 0.18
        _RimDarken("Rim darkening", Range(0, 1)) = 0.45
        [NoScaleOffset] _ReflectionTex("Reflection (runtime)", 2D) = "black" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }

        Pass
        {
            Name "Mirror"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _HazeColor;
                half _Haze;
                half _RimDarken;
            CBUFFER_END
            TEXTURE2D(_ReflectionTex);
            SAMPLER(sampler_ReflectionTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 screen = GetNormalizedScreenSpaceUV(input.positionCS);
                // The reflection camera is a regular right-handed camera, so its picture is the mirror image flipped.
                half3 reflection = SAMPLE_TEXTURE2D(_ReflectionTex, sampler_ReflectionTex, float2(1.0 - screen.x, screen.y)).rgb;
                half3 colour = lerp(reflection * _BaseColor.rgb, _HazeColor.rgb, _Haze);
                // Darker, duller towards the edge of the disc, like a hand-polished face.
                half rim = saturate(length(input.uv - 0.5) * 2.0);
                colour *= 1.0 - _RimDarken * rim * rim;
                return half4(colour, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes { float4 positionOS : POSITION; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS : SV_POSITION; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }
            half Frag(Varyings input) : SV_Target { return input.positionCS.z; }
            ENDHLSL
        }
    }
}
