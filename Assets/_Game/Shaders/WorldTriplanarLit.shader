// World-space triplanar PBR surface for blockout architecture and ground built from primitives.
// Textures tile by world metres (independent of object scale), a low-frequency resample breaks up repetition
// and an optional grime band darkens wall bases. Reuses URP Lit's material layout, so the shadow, depth and
// depth-normal passes come straight from URP and the material stays SRP-Batcher compatible.
//   _BaseMap_ST.x      tiles per metre
//   _Parallax          macro variation strength (0..1)
//   _OcclusionStrength grime strength at the base of vertical surfaces (0..1)
// Optional, per material (off by default, so existing materials render as before):
//   _ARM_MAP                _MetallicGlossMap holds a packed AO/Roughness/Metal map (R/G/B); smoothness becomes
//                           _Smoothness * (1 - roughness) and the AO channel feeds occlusion.
//   _GRAIN_FOLLOWS_OBJECT   wood: the projections turn so the texture's U axis (its grain) runs along the object's
//                           length. The length axis comes from UV3 when the mesh carries one (planed boards), else the
//                           object's longest scaled axis, else its up axis (turned and lathe-made parts).
//   _DAMP_BASE              damp creeping up from wet ground: vertical faces below world height _ClearCoatMask (m) are
//                           darker and glossier, under a ragged drying line.
//   _DUST_TOP               dust settled on upward faces, as much as _ClearCoatSmoothness (0..1).
// Both reuse URP Lit's clear-coat slots so the material keeps URP's per-material layout (SRP Batcher).
Shader "ThuyKieu/WorldTriplanarLit"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1,1,1,1)
        [Normal] _BumpMap("Normal (OpenGL)", 2D) = "bump" {}
        [NoScaleOffset] _MetallicGlossMap("AO / Roughness / Metal (with _ARM_MAP)", 2D) = "white" {}
        _BumpScale("Normal strength", Range(0, 2)) = 1
        _Smoothness("Smoothness", Range(0, 1)) = 0.2
        _Metallic("Metallic", Range(0, 1)) = 0
        _Parallax("Macro variation", Range(0, 1)) = 0.35
        _OcclusionStrength("Base grime", Range(0, 1)) = 0
        _Cutoff("Unused cutoff", Range(0, 1)) = 0.5
        _ClearCoatMask("Damp line height, m (with _DAMP_BASE)", Float) = 0.6
        _ClearCoatSmoothness("Dust amount (with _DUST_TOP)", Range(0, 1)) = 0.5
        [HideInInspector] _Surface("__surface", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "Queue" = "Geometry" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

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
            #pragma shader_feature_local_fragment _ARM_MAP
            #pragma shader_feature_local _GRAIN_FOLLOWS_OBJECT
            #pragma shader_feature_local_fragment _DAMP_BASE
            #pragma shader_feature_local_fragment _DUST_TOP

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                float3 grainOS : TEXCOORD3;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                float3 grainWS : TEXCOORD2;
                #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings Vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                float3 grain = input.grainOS;
                if (dot(grain, grain) < 0.01)
                {
                    // No grain in the mesh: the longest scaled axis, or up when the scale is (nearly) uniform.
                    float3 scale = float3(length(UNITY_MATRIX_M._m00_m10_m20), length(UNITY_MATRIX_M._m01_m11_m21), length(UNITY_MATRIX_M._m02_m12_m22));
                    float longest = max(scale.x, max(scale.y, scale.z));
                    float shortest = min(scale.x, min(scale.y, scale.z));
                    grain = longest < shortest * 1.15 ? float3(0, 1, 0)
                          : scale.y >= longest ? float3(0, 1, 0) : scale.x >= longest ? float3(1, 0, 0) : float3(0, 0, 1);
                }
                output.grainWS = TransformObjectToWorldDir(grain);
                #endif
                return output;
            }

            half3 UnpackPlane(float2 uv)
            {
                return UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
            }

            // One projection plane with axes (a, b): turns the texture so its U axis follows the grain's direction in the
            // plane. A grain (nearly) square to the plane is end grain: the plane keeps its default orientation.
            float2 GrainAxis(float2 grainInPlane)
            {
                float len = length(grainInPlane);
                return len > 0.5 ? grainInPlane / len : float2(1, 0);
            }

            float2 Turn(float2 ab, float2 axis)
            {
                return float2(dot(ab, axis), dot(ab, float2(-axis.y, axis.x)));
            }

            // A tangent-space normal sampled in the turned frame, back in the plane's (a, b) frame.
            half3 TurnBack(half3 t, float2 axis)
            {
                return half3(t.x * axis.x - t.y * axis.y, t.x * axis.y + t.y * axis.x, t.z);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 n = normalize(input.normalWS);
                float3 w = pow(abs(n), 4.0);
                w /= (w.x + w.y + w.z);
                float tiling = max(_BaseMap_ST.x, 0.0001);
                float3 p = input.positionWS * tiling;
                float2 uvX = p.zy, uvY = p.xz, uvZ = p.xy;
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                float3 g = normalize(input.grainWS);
                float2 axisX = GrainAxis(g.zy), axisY = GrainAxis(g.xz), axisZ = GrainAxis(g.xy);
                uvX = Turn(uvX, axisX); uvY = Turn(uvY, axisY); uvZ = Turn(uvZ, axisZ);
                #endif

                half3 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvX).rgb * w.x
                             + SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvY).rgb * w.y
                             + SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uvZ).rgb * w.z;

                // Low-frequency resample: large soft patches of lighter/darker colour hide the tiling grid.
                float3 q = input.positionWS * tiling * 0.071;
                float2 macroUV = q.xz + q.y * 0.3;
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                // Wood: the patches stretch along the grain too, not diagonally across it.
                macroUV = (uvX * w.x + uvY * w.y + uvZ * w.z) * 0.071;
                #endif
                half macro = dot(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, macroUV).rgb, half3(0.333, 0.333, 0.333));
                albedo *= lerp(1.0, saturate(0.55 + macro * 1.1), _Parallax);

                // Whiteout-blended tangent normals per projection, swizzled back to world space.
                half3 tX = UnpackPlane(uvX), tY = UnpackPlane(uvY), tZ = UnpackPlane(uvZ);
                #if defined(_GRAIN_FOLLOWS_OBJECT)
                tX = TurnBack(tX, axisX); tY = TurnBack(tY, axisY); tZ = TurnBack(tZ, axisZ);
                #endif
                tX = half3(tX.xy + n.zy, abs(tX.z) * n.x);
                tY = half3(tY.xy + n.xz, abs(tY.z) * n.y);
                tZ = half3(tZ.xy + n.xy, abs(tZ.z) * n.z);
                float3 normalWS = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

                // Grime: dirt rising from the ground on walls and pillars.
                half vertical = 1.0 - saturate(abs(n.y) * 3.0);
                half grime = vertical * _OcclusionStrength * (1.0 - smoothstep(0.0, 0.9, input.positionWS.y));
                albedo *= 1.0 - grime * 0.45;
                #if defined(_DAMP_BASE)
                half dampLine = _ClearCoatMask + (macro - 0.5h) * 0.2h;
                half damp = vertical * (1.0h - smoothstep(dampLine - 0.12h, dampLine + 0.04h, input.positionWS.y));
                albedo *= 1.0h - damp * 0.42h;
                #endif
                #if defined(_DUST_TOP)
                half dust = saturate((n.y - 0.6h) / 0.3h) * _ClearCoatSmoothness * saturate(0.35h + macro);
                albedo = lerp(albedo, half3(0.4h, 0.37h, 0.33h), dust * 0.65h);
                #endif

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                // Fog per pixel: ground slabs and hills have vertices hundreds of metres apart.
                inputData.fogCoord = ComputeFogFactor(TransformWorldToHClip(input.positionWS).z);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo * _BaseColor.rgb;
                surface.metallic = _Metallic;
                surface.smoothness = _Smoothness * (1.0 - grime);
                surface.normalTS = half3(0, 0, 1);
                surface.occlusion = 1.0;
                #if defined(_ARM_MAP)
                half3 arm = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uvX).rgb * w.x
                          + SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uvY).rgb * w.y
                          + SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uvZ).rgb * w.z;
                surface.smoothness *= 1.0 - arm.g;
                surface.occlusion = arm.r;
                #endif
                #if defined(_DAMP_BASE)
                surface.smoothness = lerp(surface.smoothness, 0.6h, damp);
                #endif
                #if defined(_DUST_TOP)
                surface.smoothness *= 1.0h - dust * 0.85h;
                #endif
                surface.alpha = 1.0;
                surface.specular = half3(0, 0, 0);

                half4 color = UniversalFragmentPBR(inputData, surface);
                color.rgb = MixFog(color.rgb, inputData.fogCoord);
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On ColorMask R Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/RenderingLayers.hlsl"
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
