Shader "ClawMachine/ToonOutline"
{
    Properties
    {
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0.10, 0.10, 0.14, 1)
        _OutlineWidth ("Outline Width", Range(0.005, 0.06)) = 0.020
        _CelThreshold ("Cel Threshold", Range(0.2, 0.8)) = 0.40
        _CelSoftness ("Cel Softness", Range(0.005, 0.08)) = 0.035
        _SpecThreshold ("Spec Threshold", Range(0.85, 0.99)) = 0.94
        _AmbientBoost ("Ambient Level", Range(0.2, 0.6)) = 0.42
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        // PASS 1: OUTLINE (Inverted Hull with View-Space Extrusion)
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZWrite On
            ZTest LEqual
            Offset 1, 1

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float _OutlineWidth;
            fixed4 _OutlineColor;

            v2f vert(appdata v)
            {
                v2f o;
                // Transform vertex position and normal to view space (camera coordinate system)
                float4 viewPos = mul(UNITY_MATRIX_MV, v.vertex);
                float3 viewNorm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
                
                // Extrude vertices along view normal in view space (scale-independent!)
                viewPos.xyz += normalize(viewNorm) * _OutlineWidth;
                
                // Project to clip space
                o.pos = mul(UNITY_MATRIX_P, viewPos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // PASS 2: FORWARD BASE (Stylized Cel Shading)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode"="ForwardBase" }
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float2 uv : TEXCOORD3;
                LIGHTING_COORDS(4, 5)
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _CelThreshold;
            float _CelSoftness;
            float _SpecThreshold;
            float _AmbientBoost;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                TRANSFER_VERTEX_TO_FRAGMENT(o);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 halfDir = normalize(lightDir + viewDir);

                // Half-Lambert wrap for smooth curvature without harsh terminator
                float NdotL = saturate(dot(normal, lightDir) * 0.5 + 0.5);
                
                // Clean 2-band stepped cel shading
                float band1 = smoothstep(_CelThreshold - _CelSoftness, _CelThreshold + _CelSoftness, NdotL);
                float band2 = smoothstep(0.72 - _CelSoftness, 0.72 + _CelSoftness, NdotL);
                float lightFactor = lerp(_AmbientBoost, 0.75, band1);
                lightFactor = lerp(lightFactor, 1.0, band2);

                // Stepped specular highlight
                float NdotH = max(0.0, dot(normal, halfDir));
                float spec = smoothstep(_SpecThreshold - 0.01, _SpecThreshold + 0.01, NdotH) * 0.40;

                UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);

                fixed4 texCol = tex2D(_MainTex, i.uv) * _Color;
                float3 directLight = _LightColor0.rgb * lightFactor * atten;
                float3 ambient = UNITY_LIGHTMODEL_AMBIENT.rgb * _AmbientBoost;
                float3 finalRgb = texCol.rgb * (directLight + ambient) + spec;

                return fixed4(finalRgb, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
