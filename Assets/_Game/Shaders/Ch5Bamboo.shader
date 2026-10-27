// Bamboo of the Chapter 5 forest (only there): URP Lit's surface (albedo, normal map, smoothness from the albedo's alpha,
// alpha clip for leaves, two-sided) with the night wind in it. The mesh's vertex colour says how each vertex moves:
//   r  bend    0 at the foot of a culm, rising to 1 at its tip: the whole culm leans and swings slowly, the foot never moves
//   g  flutter 0 on culms, 1 on leaves: leaves and twigs shiver faster, on top of the bend of the culm they hang from
//   b  phase   one value per culm, so neighbours never swing together
// The wind is the global _C5Wind (xz direction, strength in z... see ForestWind): with none set nothing moves, so the
// material renders as still bamboo anywhere else. Meshes built without colours (the obstacles' culms) stay still too.
// Leaves let a little of the moon through from behind (_ClearCoatSmoothness), dimly, never glowing.
// Reuses URP Lit's material layout (SRP Batcher compatible):
//   _Parallax       how far a culm's tip swings (m at full wind)
//   _ClearCoatMask  how far leaves flutter (m)
//   _ClearCoatSmoothness  light through the leaves from behind (0..1)
Shader "ThuyKieu/Ch5Bamboo"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1,1,1,1)
        [Normal] _BumpMap("Normal", 2D) = "bump" {}
        _BumpScale("Normal strength", Range(0, 2)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 0.4
        _Metallic("Metallic", Range(0, 1)) = 0
        _Cutoff("Alpha cutoff", Range(0, 1)) = 0.45
        _Parallax("Sway (m)", Range(0, 2)) = 0.35
        _ClearCoatMask("Flutter (m)", Range(0, 0.2)) = 0.04
        _ClearCoatSmoothness("Light through leaves", Range(0, 1)) = 0.25
        [HideInInspector] _Cull("__cull", Float) = 2
        [HideInInspector] _SmoothnessTextureChannel("__smoothnessChannel", Float) = 0
        [HideInInspector] _Surface("__surface", Float) = 0
        [HideInInspector] _AlphaClip("__clip", Float) = 0
        [HideInInspector] _AlphaToMask("__alphaToMask", Float) = 0
    }

    HLSLINCLUDE
    #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"

    float4 _C5Wind; // xy: direction (unit, world xz) · z: strength 0..1 · w: gusts per second

    float3 Wind(float3 positionWS, half4 weights)
    {
        float strength = _C5Wind.z;
        if (strength <= 0.0) return positionWS;
        float t = _Time.y, phase = weights.b * 6.2831853;
        float2 dir = _C5Wind.xy;
        // The culm: a slow lean with the wind and a swing about it, larger in gusts; its tip moves the most.
        float gust = 0.55 + 0.45 * sin(t * _C5Wind.w + phase * 0.3 + positionWS.x * 0.02);
        float swing = 0.45 + 0.35 * sin(t * 0.83 + phase) + 0.2 * sin(t * 1.91 + phase * 1.7);
        float bend = weights.r * weights.r * _Parallax * strength * gust * swing;
        float2 across = float2(-dir.y, dir.x) * 0.3 * sin(t * 0.61 + phase * 2.3);
        positionWS.xz += (dir + across) * bend;
        positionWS.y -= bend * bend * 0.12; // a bent culm is a little shorter
        // Leaves: a quick shiver, never in step with the next.
        float flutter = weights.g * _ClearCoatMask * strength * (0.6 + 0.4 * gust);
        float k = t * 6.3 + phase * 11.0 + dot(positionWS, float3(1.7, 2.3, 1.1));
        positionWS += float3(sin(k), sin(k * 1.31 + 1.3) * 0.6, cos(k * 0.87)) * flutter;
        return positionWS;
    }
    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull [_Cull]
            // Leaves: their cut-out edge becomes MSAA coverage, so it is smoothed like a polygon's edge.
            AlphaToMask [_AlphaToMask]

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local_fragment _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _LIGHT_COOKIES
            #pragma multi_compile _ _LIGHT_LAYERS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                half4 tangentWS : TEXCOORD3;
                half leaf : TEXCOORD4;
                half fog : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                float3 positionWS = Wind(TransformObjectToWorld(input.positionOS.xyz), input.color);
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                VertexNormalInputs normals = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.normalWS = normals.normalWS;
                output.tangentWS = half4(normals.tangentWS, input.tangentOS.w * GetOddNegativeScale());
                output.leaf = input.color.g;
                // Fog from the clip-space depth here (in the fragment SV_POSITION.z is window depth, not what fog wants).
                output.fog = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                SurfaceData surface;
                InitializeStandardLitSurfaceData(input.uv, surface);

                half3 normalWS = normalize(input.normalWS);
                #if defined(_NORMALMAP)
                half3 bitangent = input.tangentWS.w * cross(normalWS, input.tangentWS.xyz);
                normalWS = normalize(TransformTangentToWorld(surface.normalTS, half3x3(input.tangentWS.xyz, bitangent, normalWS)));
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = input.fog;
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                half4 color = UniversalFragmentPBR(inputData, surface);
                // Moonlight through a leaf seen against it: a little, in the leaf's own colour.
                if (input.leaf > 0.01h && _ClearCoatSmoothness > 0.0h)
                {
                    Light moon = GetMainLight(inputData.shadowCoord);
                    #if defined(_LIGHT_COOKIES)
                    moon.color *= SampleMainLightCookie(input.positionWS);
                    #endif
                    half back = pow(saturate(dot(inputData.viewDirectionWS, -moon.direction)), 4.0h);
                    color.rgb += surface.albedo * moon.color * moon.shadowAttenuation * back * _ClearCoatSmoothness * input.leaf;
                }
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                color.a = surface.alpha;
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings ShadowVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 positionWS = Wind(TransformObjectToWorld(input.positionOS.xyz), input.color);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                float3 lightDirectionWS = _LightDirection;
                #endif
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                #if UNITY_REVERSED_Z
                positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                output.positionCS = positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings DepthVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformWorldToHClip(Wind(TransformObjectToWorld(input.positionOS.xyz), input.color));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            half DepthFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                return input.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex NormalsVert
            #pragma fragment NormalsFrag
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings NormalsVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = TransformWorldToHClip(Wind(TransformObjectToWorld(input.positionOS.xyz), input.color));
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 NormalsFrag(Varyings input, bool front : SV_IsFrontFace) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Alpha(SampleAlbedoAlpha(input.uv, TEXTURE2D_ARGS(_BaseMap, sampler_BaseMap)).a, _BaseColor, _Cutoff);
                half3 normalWS = NormalizeNormalPerPixel(front ? input.normalWS : -input.normalWS);
                #if defined(_GBUFFER_NORMALS_OCT)
                float2 octNormalWS = PackNormalOctQuadEncode(normalWS);
                return half4(PackFloat2To888(saturate(octNormalWS * 0.5 + 0.5)), 0.0);
                #else
                return half4(normalWS, 0.0);
                #endif
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
