// Soft warm glow added over a surface for objective guidance (GuideGlow): lamplight through paper, light through the
// crack of a door, a gleam on metal. Additive and unlit, so it lights nothing else and no Light is involved; it fades
// into the fog with the rest of the scene. Every value comes from the GuideGlow component (material property block):
//   _Intensity  0 = invisible (the default, so the quads never show in the editor or before the component runs)
//   _Edge       soft border across and along the quad, as a share of its half-size (panel shape)
//   _Round      1 = a round gleam fading out from its middle, 0 = a soft-edged panel brighter low in the middle
Shader "ThuyKieu/GuideGlow"
{
    Properties
    {
        _Color("Colour", Color) = (1, 0.74, 0.45, 1)
        _Intensity("Intensity", Float) = 0
        _Edge("Edge softness (x across, y along)", Vector) = (0.3, 0.3, 0, 0)
        _Round("Round gleam", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" "IgnoreProjector" = "True" }

        Pass
        {
            Name "GuideGlow"
            Tags { "LightMode" = "UniversalForward" }
            Blend One One
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Intensity;
                half4 _Edge;
                half _Round;
            CBUFFER_END

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
                half fog : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 p = input.uv * 2.0 - 1.0;
                // Panel: soft borders, and a little brighter low in the middle, where a lamp behind paper would stand.
                float2 edge = smoothstep(0.0, 1.0, saturate((1.0 - abs(p)) / max(_Edge.xy, 0.001)));
                float panel = edge.x * edge.y * (0.7 + 0.3 * saturate(1.0 - length(p - float2(0.0, -0.25))));
                // Gleam: brightest in the middle, gone at the rim.
                float gleam = saturate(1.0 - length(p));
                float shape = lerp(panel, gleam * gleam, _Round);
                half3 colour = _Color.rgb * (_Intensity * shape);
                // Additive: the fog takes it towards nothing, as it takes the lamplit surfaces behind it.
                colour = MixFogColor(colour, half3(0, 0, 0), input.fog);
                return half4(colour, 0);
            }
            ENDHLSL
        }
    }
}
