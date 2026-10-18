Shader "ClawMachine/ArcadeGlass"
{
    Properties
    {
        [MainColor] _Color ("Glass Tint", Color) = (0.78, 0.94, 1.0, 0.06)
        _RimColor ("Edge Fresnel Color", Color) = (0.55, 0.90, 1.0, 0.42)
        _FresnelPower ("Fresnel Power", Range(1.0, 8.0)) = 3.5
        _SweepIntensity ("Diagonal Reflection Streak", Range(0.0, 0.5)) = 0.14
        _SweepWidth ("Streak Width", Range(0.02, 0.35)) = 0.12
        _SpecPower ("Specular Sharpness", Range(16.0, 256.0)) = 96.0
        _SpecIntensity ("Specular Intensity", Range(0.0, 1.0)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
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

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half4 _RimColor;
                half _FresnelPower;
                half _SweepIntensity;
                half _SweepWidth;
                half _SpecPower;
                half _SpecIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = SafeNormalize(GetCameraPositionWS() - input.positionWS);

                // 1. Fresnel edge highlight (tempered acrylic/glass rim)
                float NdotV = saturate(dot(normalWS, viewDirWS));
                half fresnel = pow(1.0 - NdotV, _FresnelPower);

                // 2. Subtle diagonal arcade glass reflection band
                float diagCoord = frac((input.positionWS.x * 0.35 + input.positionWS.y * 0.45) - viewDirWS.x * 0.25);
                half streak1 = smoothstep(0.5 - _SweepWidth, 0.5, diagCoord) * (1.0 - smoothstep(0.5, 0.5 + _SweepWidth, diagCoord));
                half streak2 = smoothstep(0.72 - _SweepWidth * 0.5, 0.72, diagCoord) * (1.0 - smoothstep(0.72, 0.72 + _SweepWidth * 0.5, diagCoord));
                half sheen = (streak1 + streak2 * 0.6) * _SweepIntensity;

                // 3. Crisp specular glint from main light
                Light mainLight = GetMainLight();
                float3 halfDir = SafeNormalize(normalize(mainLight.direction) + viewDirWS);
                half spec = pow(saturate(dot(normalWS, halfDir)), _SpecPower) * _SpecIntensity;

                half3 rgb = lerp(_Color.rgb, _RimColor.rgb, fresnel) + half3(sheen + spec, sheen + spec, sheen + spec);
                half alpha = saturate(_Color.a + fresnel * _RimColor.a + sheen * 0.65 + spec * 0.5);

                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Unlit"
}
