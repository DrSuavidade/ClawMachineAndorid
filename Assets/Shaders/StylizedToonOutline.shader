Shader "ClawMachine/ToonOutline"
{
    Properties
    {
        [MainColor] _Color ("Main Color", Color) = (1, 1, 1, 1)
        _BaseColor ("Base Color (URP Alias)", Color) = (1, 1, 1, 1)
        [MainTexture] _MainTex ("Base Texture", 2D) = "white" {}

        [Header(Stylized Cel Shading)]
        _CelThreshold ("Cel Threshold", Range(0.15, 0.85)) = 0.42
        _CelSoftness ("Cel Softness", Range(0.005, 0.15)) = 0.045
        _AmbientBoost ("Shadow Brightness", Range(0.20, 0.75)) = 0.46
        _ShadowTint ("Shadow Warm/Cool Tint", Color) = (0.72, 0.68, 0.86, 1)

        [Header(Specular and Metallic)]
        _Metallic ("Metallic", Range(0.0, 1.0)) = 0.0
        _Glossiness ("Smoothness", Range(0.0, 1.0)) = 0.45
        _Smoothness ("Smoothness (URP Alias)", Range(0.0, 1.0)) = 0.45
        _SpecThreshold ("Specular Cutoff", Range(0.80, 0.995)) = 0.93
        _SpecIntensity ("Specular Intensity", Range(0.0, 1.5)) = 0.40

        [Header(Fresnel Rim Pop)]
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimPower ("Rim Power", Range(1.0, 8.0)) = 3.2
        _RimIntensity ("Rim Intensity", Range(0.0, 1.0)) = 0.28

        [Header(Emission)]
        [HDR] _EmissionColor ("Emission Color", Color) = (0, 0, 0, 1)

        [Header(Adaptive Outline)]
        _OutlineColor ("Outline Color", Color) = (0.10, 0.09, 0.14, 1)
        _OutlineColorTint ("Outline Surface Tint Blend", Range(0.0, 1.0)) = 0.55
        _OutlineWidth ("Outline Width", Range(0.0, 0.04)) = 0.014
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _Color;
            half4 _BaseColor;
            half4 _ShadowTint;
            half4 _RimColor;
            half4 _EmissionColor;
            half4 _OutlineColor;
            half _CelThreshold;
            half _CelSoftness;
            half _AmbientBoost;
            half _Metallic;
            half _Glossiness;
            half _Smoothness;
            half _SpecThreshold;
            half _SpecIntensity;
            half _RimPower;
            half _RimIntensity;
            half _OutlineColorTint;
            float _OutlineWidth;
        CBUFFER_END
        ENDHLSL

        // =====================================================================
        // PASS 1: FORWARD LIT (3-Band Stylized Cel + Point/Spot Lights + Rim + Metallic)
        // =====================================================================
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs posInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionWS = posInputs.positionWS;
                output.positionCS = posInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half3 EvaluateStylizedLight(Light light, float3 normalWS, float3 viewDirWS, half3 albedo, half metallic, half smoothness, bool isMainLight)
            {
                float3 lightDir = normalize(light.direction);
                half atten = light.distanceAttenuation * light.shadowAttenuation;

                // Half-Lambert for main light so toys never have harsh black backs; standard NdotL for point/spot lights
                float rawNdotL = dot(normalWS, lightDir);
                float wrappedNdotL = isMainLight ? saturate(rawNdotL * 0.5 + 0.5) : saturate(rawNdotL);

                // 2-step cel ramp (shadow -> midtone -> highlight band)
                half band1 = smoothstep(_CelThreshold - _CelSoftness, _CelThreshold + _CelSoftness, wrappedNdotL);
                half band2 = smoothstep(0.74 - _CelSoftness, 0.74 + _CelSoftness, wrappedNdotL);

                half3 rampColor;
                if (isMainLight)
                {
                    // Warm lit bands vs cool tinted shadow band
                    half3 shadowTone = _ShadowTint.rgb * _AmbientBoost;
                    half3 midTone = lerp(shadowTone, half3(0.86, 0.86, 0.88), band1);
                    rampColor = lerp(midTone, half3(1.0, 1.0, 1.0), band2);
                    // Factor shadow attenuation smoothly into the cel ramp
                    rampColor = lerp(shadowTone, rampColor, saturate(atten));
                }
                else
                {
                    // Point / Spot light additive cel contribution
                    half addBand = smoothstep(0.15, 0.55, wrappedNdotL) * atten;
                    rampColor = half3(addBand, addBand, addBand);
                }

                // Stylized Specular (scales with smoothness & metallic)
                float3 halfDir = SafeNormalize(lightDir + viewDirWS);
                float NdotH = saturate(dot(normalWS, halfDir));
                half effectiveSpecCutoff = lerp(_SpecThreshold, 0.86, metallic * 0.6 + smoothness * 0.3);
                half specSoftness = lerp(0.015, 0.006, smoothness);
                half specMask = smoothstep(effectiveSpecCutoff - specSoftness, effectiveSpecCutoff + specSoftness, NdotH);
                specMask *= saturate(rawNdotL + 0.15) * atten;

                // Metallic surfaces tint specular with their own base color (gold/bronze/tinted chrome)
                half3 specColor = lerp(half3(1.0, 1.0, 1.0), albedo * 1.35, metallic * 0.75);
                half specStrength = _SpecIntensity * (0.5 + smoothness * 0.8 + metallic * 0.7);

                half3 diffuseTerm = albedo * lerp(1.0, 0.65, metallic) * rampColor * light.color;
                half3 specTerm = specColor * (specMask * specStrength) * light.color;

                return diffuseTerm + specTerm;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);

                // Support both _Color and _BaseColor (for MaterialPropertyBlock overrides)
                half4 tint = _Color;
                if (abs(_BaseColor.r - 1.0) + abs(_BaseColor.g - 1.0) + abs(_BaseColor.b - 1.0) > 0.001 &&
                    abs(_Color.r - 1.0) + abs(_Color.g - 1.0) + abs(_Color.b - 1.0) < 0.001)
                {
                    tint = _BaseColor;
                }

                half4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * tint;
                half3 albedo = texSample.rgb;

                half smoothness = max(_Glossiness, _Smoothness);
                half metallic = _Metallic;

                // 1. Main Directional Light (with real-time shadows)
                float4 shadowCoord = TransformWorldToShadowCoord(input.positionWS);
                Light mainLight = GetMainLight(shadowCoord);
                half3 lighting = EvaluateStylizedLight(mainLight, normalWS, viewDirWS, albedo, metallic, smoothness, true);

                // 2. Additional Lights (Spotlights, Neon Point Lights, Preview Rig Lights)
                #ifdef _ADDITIONAL_LIGHTS
                uint pixelLightCount = GetAdditionalLightsCount();
                for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
                {
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS, half4(1, 1, 1, 1));
                    lighting += EvaluateStylizedLight(addLight, normalWS, viewDirWS, albedo, metallic, smoothness, false);
                }
                #endif

                // 3. Spherical Harmonics Ambient + Stylized Metallic Faux-Environment Sheen
                half3 shAmbient = SampleSH(normalWS) * (_AmbientBoost * 0.65);
                if (metallic > 0.01)
                {
                    // Faux studio sky/horizon reflection for gold, silver, and chrome parts
                    float3 reflDir = reflect(-viewDirWS, normalWS);
                    half horizonBand = smoothstep(-0.2, 0.35, reflDir.y);
                    half zenithBand = smoothstep(0.55, 0.85, reflDir.y);
                    half3 envSheen = lerp(albedo * 0.35, albedo * 1.15, horizonBand) + zenithBand * 0.35;
                    shAmbient += envSheen * (metallic * (0.45 + smoothness * 0.45));
                }

                // 4. Fresnel Rim Light (masked by upward/lit hemisphere so undersides don't glow unnaturally)
                float NdotV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0 - NdotV, _RimPower);
                half rimMask = saturate(dot(normalWS, normalize(mainLight.direction)) * 0.5 + 0.5) * saturate(normalWS.y * 0.5 + 0.65);
                half3 rimTint = lerp(_RimColor.rgb, albedo * 1.25, 0.45) * mainLight.color;
                half3 rimLight = fresnel * rimMask * _RimIntensity * rimTint;

                // 5. Combine & Add HDR Emission
                half3 finalRGB = lighting + (albedo * shAmbient) + rimLight + _EmissionColor.rgb;
                return half4(finalRGB, texSample.a);
            }
            ENDHLSL
        }

        // =====================================================================
        // PASS 2: ADAPTIVE INVERTED-HULL OUTLINE (SRPDefaultUnlit)
        // =====================================================================
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vertOutline(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

                // Distance-stabilized outline thickness so close-up camera zoom doesn't bloat lines
                float distToCam = distance(positionWS, GetCameraPositionWS());
                float distScale = clamp(distToCam * 0.16, 0.55, 1.25);

                // Extrude along world normal + slight view-depth bias to prevent inner-part z-fighting
                float3 viewDir = SafeNormalize(GetCameraPositionWS() - positionWS);
                positionWS += normalWS * (_OutlineWidth * distScale) - viewDir * 0.0025;

                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }

            half4 fragOutline(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Blend base dark outline color with darkened, rich tint of the surface color
                half3 surfColor = _Color.rgb;
                half3 tintedLine = surfColor * surfColor * 0.28;
                half3 finalOutline = lerp(_OutlineColor.rgb, tintedLine, _OutlineColorTint);
                return half4(finalOutline, 1.0);
            }
            ENDHLSL
        }

        // =====================================================================
        // PASS 3: SHADOW CASTER
        // =====================================================================
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float3 _LightDirection;

            Varyings vertShadow(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    output.positionCS.z = min(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    output.positionCS.z = max(output.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return output;
            }

            half4 fragShadow(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // =====================================================================
        // PASS 4: DEPTH NORMALS (URP Forward+ / SSAO Compatibility)
        // =====================================================================
        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vertDepthNormals
            #pragma fragment fragDepthNormals
            #pragma multi_compile_instancing

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
            };

            Varyings vertDepthNormals(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            half4 fragDepthNormals(Varyings input) : SV_Target
            {
                return half4(NormalizeNormalPerPixel(input.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
