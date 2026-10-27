// The Chapter 5 forest floor (only there): one material for the way, its banks and the floor beyond, so nothing is laid
// on top of the ground and no seam shows where the way meets the bank. World-space like WorldTriplanarLit: the floor's
// own texture (with its AO / roughness map) from above, a drier crumbling earth on the faces from the sides. What the
// ground is like at each place comes from the mesh's vertex colour (see ForestGround.Look):
//   r trodden    the line feet keep to: darker, packed (less relief), a little smoother
//   g damp       hollows and the bottoms of cuts: darker, and glossy only where the earth's roughness map is low
//                (packed patches and small stones), so damp reads as patches of sheen, never one coat of varnish;
//                where it is wettest, small soft-edged pools in the earth's own hollows
//   b bare       (the faces; the side texture follows the normal by itself)
//   a leaves     fallen leaves on the floor: towards the leaf colour, flatter, matte
// Two scales of the floor texture, blended by large soft patches, keep its repeat from showing; one more very low
// resample tints the ground in broad lighter and darker areas.
// It reuses URP Lit's material layout (SRP Batcher compatible; shadow and depth passes from URP):
//   _BaseMap, _BumpMap, _MetallicGlossMap   floor albedo, normal (OpenGL), AO (r) / roughness (g)  · _BaseMap_ST.x tiles per metre
//   _DetailAlbedoMap, _DetailNormalMap       face albedo and normal · _DetailAlbedoMap_ST.x tiles per metre
//   _BumpScale, _DetailNormalMapScale       floor / face normal strength
//   _Smoothness                             smoothness of dry, smooth earth (times 1 − roughness)
//   _OcclusionStrength                      how much of the AO map is used
//   _Parallax                               broad tint variation (0..1)
//   _SpecColor, _EmissionColor              trodden tint, leaf colour
//   _ClearCoatMask, _DetailAlbedoMapScale   damp: its sheen, how much darker
//   _ClearCoatSmoothness                    how much the fallen leaves cover where they lie
Shader "ThuyKieu/Ch5ForestGround"
{
    Properties
    {
        [MainTexture] _BaseMap("Floor albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1,1,1,1)
        [Normal] _BumpMap("Floor normal (OpenGL)", 2D) = "bump" {}
        [NoScaleOffset] _MetallicGlossMap("Floor AO (r) / roughness (g)", 2D) = "white" {}
        _BumpScale("Floor normal strength", Range(0, 2)) = 1
        _DetailAlbedoMap("Face albedo", 2D) = "white" {}
        [Normal] _DetailNormalMap("Face normal (OpenGL)", 2D) = "bump" {}
        _DetailNormalMapScale("Face normal strength", Range(0, 2)) = 1
        _Smoothness("Smoothness (dry)", Range(0, 1)) = 0.3
        _Metallic("Metallic", Range(0, 1)) = 0
        _OcclusionStrength("AO strength", Range(0, 1)) = 0.8
        _Parallax("Broad variation", Range(0, 1)) = 0.4
        _SpecColor("Trodden tint", Color) = (0.82, 0.78, 0.74, 1)
        _EmissionColor("Leaf colour", Color) = (0.36, 0.27, 0.17, 1)
        _ClearCoatMask("Damp sheen", Range(0, 1)) = 0.5
        _DetailAlbedoMapScale("Damp darkening", Range(0, 1)) = 0.3
        _ClearCoatSmoothness("Leaf cover", Range(0, 1)) = 0.55
        _Cutoff("Unused cutoff", Range(0, 1)) = 0.5
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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                half4 color : TEXCOORD2;
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
                output.color = input.color;
                return output;
            }

            // The floor from above at one scale: albedo, tangent normal, AO / roughness.
            void Floor(float2 uv, out half3 albedo, out half3 normalTS, out half2 aoRough)
            {
                albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb;
                normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, uv), _BumpScale);
                aoRough = SAMPLE_TEXTURE2D(_MetallicGlossMap, sampler_MetallicGlossMap, uv).rg;
            }

            half3 Face(float2 uv, out half3 normalTS)
            {
                normalTS = UnpackNormalScale(SAMPLE_TEXTURE2D(_DetailNormalMap, sampler_DetailNormalMap, uv), _DetailNormalMapScale);
                return SAMPLE_TEXTURE2D(_DetailAlbedoMap, sampler_DetailAlbedoMap, uv).rgb;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half trodden = input.color.r, damp = input.color.g, leaves = input.color.a;
                float3 n = normalize(input.normalWS);
                float3 w = pow(abs(n), 4.0);
                w /= (w.x + w.y + w.z);
                float3 p = input.positionWS;
                float top = max(_BaseMap_ST.x, 0.0001), side = max(_DetailAlbedoMap_ST.x, 0.0001);

                // Broad patches (tens of metres, one low-detail read): a lighter or darker tint, and which of two scales and
                // turns of the floor texture shows. Only the bands between patches read both; only what faces up reads the
                // floor, only what faces sideways the faces.
                half3 broadRead = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, p.xz * top * 0.0073 + 0.31).rgb;
                half broad = broadRead.g;
                half pick = smoothstep(0.42h, 0.58h, broadRead.r * 0.6h + broadRead.b * 0.4h);
                half3 floorAlbedo = half3(0.5h, 0.5h, 0.5h), floorNormal = half3(0, 0, 1);
                half2 aoRough = half2(1.0h, 0.85h);
                UNITY_BRANCH
                if (w.y > 0.02)
                {
                    float2 uvA = p.xz * top;
                    float2 uvB = float2(uvA.x * 0.788 - uvA.y * 0.616, uvA.x * 0.616 + uvA.y * 0.788) * 0.63 + 0.47;
                    half3 albedoA = 0, albedoB = 0, normalA = half3(0, 0, 1), normalB = half3(0, 0, 1);
                    half2 armA = 0, armB = 0;
                    UNITY_BRANCH
                    if (pick < 0.995h) Floor(uvA, albedoA, normalA, armA);
                    UNITY_BRANCH
                    if (pick > 0.005h) Floor(uvB, albedoB, normalB, armB);
                    floorAlbedo = lerp(albedoA, albedoB, pick);
                    floorNormal = normalize(lerp(normalA, half3(normalB.x * 0.788 + normalB.y * 0.616, -normalB.x * 0.616 + normalB.y * 0.788, normalB.z), pick));
                    aoRough = lerp(armA, armB, pick);
                }

                half3 faceNormalX = half3(0, 0, 1), faceNormalZ = half3(0, 0, 1), faceAlbedo = 0;
                UNITY_BRANCH
                if (w.x > 0.02) faceAlbedo += Face(p.zy * side, faceNormalX) * w.x;
                UNITY_BRANCH
                if (w.z > 0.02) faceAlbedo += Face(p.xy * side, faceNormalZ) * w.z;
                half3 albedo = (floorAlbedo * w.y + faceAlbedo) / max(w.x + w.y + w.z, 1e-4);
                albedo *= lerp(1.0h, saturate(0.62h + broad * 0.9h), _Parallax);

                // Packed down where feet go, damp where water stands, leaves where nobody walks.
                half relief = (1.0h - 0.35h * trodden) * (1.0h - 0.55h * leaves);
                floorNormal = normalize(half3(floorNormal.xy * relief, floorNormal.z));
                albedo *= lerp(half3(1, 1, 1), _SpecColor.rgb, trodden);
                half leafy = leaves * _ClearCoatSmoothness * saturate(w.y * 1.6h - 0.3h);
                albedo = lerp(albedo, _EmissionColor.rgb * (0.55h + dot(floorAlbedo, half3(0.6h, 0.6h, 0.6h))), leafy);
                albedo *= 1.0h - damp * _DetailAlbedoMapScale;
                // Small pools where it is wettest, in the ground's own hollows (its AO): level ground only, soft-edged,
                // dark and smooth, flat on the earth instead of laid over it.
                half pool = saturate((damp - 0.7h) * 6.0h + (0.55h - aoRough.r) * 2.0h) * saturate((w.y - 0.85h) * 8.0h) * (1.0h - leafy);
                albedo *= 1.0h - 0.6h * pool;
                floorNormal = normalize(lerp(floorNormal, half3(0, 0, 1), pool));

                // Whiteout blend per projection, swizzled back to world space (as WorldTriplanarLit).
                half3 tX = half3(faceNormalX.xy + n.zy, abs(faceNormalX.z) * n.x);
                half3 tY = half3(floorNormal.xy + n.xz, abs(floorNormal.z) * n.y);
                half3 tZ = half3(faceNormalZ.xy + n.xy, abs(faceNormalZ.z) * n.z);
                float3 normalWS = normalize(tX.zyx * w.x + tY.xzy * w.y + tZ.xyz * w.z);

                half rough = lerp(0.85h, aoRough.g, w.y);
                // The trodden line keeps a little of the rain's sheen (more on its smoother crumbs), the leaves none.
                half smoothness = (_Smoothness * (1.0h - rough) + 0.3h * trodden * (1.0h - 0.5h * rough)) * (1.0h - 0.7h * leafy);
                smoothness = lerp(smoothness, _ClearCoatMask * saturate(1.35h - rough * 1.25h), damp * (1.0h - leafy));
                smoothness = lerp(smoothness, 0.9h, pool);

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.positionCS = input.positionCS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                inputData.fogCoord = ComputeFogFactor(TransformWorldToHClip(input.positionWS).z);
                inputData.bakedGI = SampleSH(normalWS);
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);
                inputData.shadowMask = half4(1, 1, 1, 1);

                SurfaceData surface = (SurfaceData)0;
                surface.albedo = albedo * _BaseColor.rgb;
                surface.metallic = _Metallic;
                surface.smoothness = smoothness;
                surface.normalTS = half3(0, 0, 1);
                surface.occlusion = lerp(1.0h, aoRough.r, _OcclusionStrength * w.y);
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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
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
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitDepthNormalsPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
