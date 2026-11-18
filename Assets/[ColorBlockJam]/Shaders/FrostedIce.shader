Shader "ColorBlockJam/FrostedIce"
{
    Properties
    {
        [MainTexture][NoScaleOffset] _BaseMap ("Ice texture", 2D) = "white" {}
        [MainColor] _Tint ("Tint", Color) = (0.62, 0.86, 0.97, 1)
        _Glow ("Glow, how little the light shades the ice", Range(0, 1)) = 0.6
        _TilesPerUnit ("Texture repeats per world unit", Float) = 0.12
        _TextureStrength ("Texture strength", Range(0, 1)) = 0.5
        _CrackColor ("Crack colour", Color) = (1, 1, 1, 1)
        _CrackFrom ("Crack from texture brightness", Range(0, 1)) = 0.52
        _CrackTo ("Full crack at texture brightness", Range(0, 1)) = 0.6
        _EdgeColor ("Edge colour", Color) = (1, 1, 1, 1)
        _EdgePower ("Edge sharpness", Range(0.5, 8)) = 2
        _EdgeStrength ("Edge strength", Range(0, 2)) = 0.7
        _Shininess ("Highlight sharpness", Range(4, 256)) = 64
        _HighlightStrength ("Highlight strength", Range(0, 2)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "Ice"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            static const half AxisSharpness = 4;
            static const half3 LuminanceWeights = half3(0.2126, 0.7152, 0.0722);

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _Tint;
                half _Glow;
                float _TilesPerUnit;
                half _TextureStrength;
                half4 _CrackColor;
                half _CrackFrom;
                half _CrackTo;
                half4 _EdgeColor;
                half _EdgePower;
                half _EdgeStrength;
                half _Shininess;
                half _HighlightStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            Varyings Vert(Attributes input)
            {
                VertexPositionInputs position = GetVertexPositionInputs(input.positionOS.xyz);

                Varyings output;
                output.positionCS = position.positionCS;
                output.positionWS = position.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }

            // The brightness of the texture, laid on along the three world axes, not by the mesh's own
            // coordinates: the cracks run on from cell to cell and have one size on every block and door.
            half SampleIce(float3 positionWS, half3 normalWS)
            {
                float3 uv = positionWS * _TilesPerUnit;
                half3 weights = pow(abs(normalWS), AxisSharpness);
                weights /= weights.x + weights.y + weights.z;

                half3 alongX = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv.zy).rgb;
                half3 alongY = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv.xz).rgb;
                half3 alongZ = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv.xy).rgb;
                return dot(alongX * weights.x + alongY * weights.y + alongZ * weights.z, LuminanceWeights);
            }

            half4 Frag(Varyings input) : SV_Target
            {
                half3 normal = normalize(input.normalWS);
                half3 view = GetWorldSpaceNormalizeViewDir(input.positionWS);
                Light light = GetMainLight();
                half ice = SampleIce(input.positionWS, normal);

                // The texture's brightness sits around one half, so twice of it varies around the tint.
                half3 albedo = _Tint.rgb * lerp(1, ice * 2, _TextureStrength);
                half3 lighting = SampleSH(normal) + light.color * saturate(dot(normal, light.direction));
                half3 color = albedo * lerp(lighting, 1, _Glow);

                // The brightest lines of the texture are its cracks.
                color = lerp(color, _CrackColor.rgb, smoothstep(_CrackFrom, _CrackTo, ice) * _CrackColor.a);

                // Upright faces, the sides of the block and of its studs, glow: the studs show as rings.
                half edge = pow(1 - saturate(normal.y), _EdgePower) * _EdgeStrength;
                color += _EdgeColor.rgb * edge;

                half highlight = pow(saturate(dot(normal, normalize(light.direction + view))), _Shininess) * _HighlightStrength;
                color += light.color * highlight;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
