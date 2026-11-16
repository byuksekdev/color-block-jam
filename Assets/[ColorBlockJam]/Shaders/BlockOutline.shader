Shader "ColorBlockJam/BlockOutline"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Width ("Width, in world units", Float) = 0.1
    }

    SubShader
    {
        // After the board, so the edge lies over whatever stands next to the block.
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry+10" "RenderPipeline" = "UniversalPipeline" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            float _Width;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
        };
        ENDHLSL

        // URP draws the SRPDefaultUnlit pass of a shader before its UniversalForward pass.
        // This one marks every pixel the block covers and draws nothing.
        Pass
        {
            Name "Cover"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            ColorMask 0
            ZWrite Off
            ZTest Always

            // The highest bit, which the masks of the UI do not use.
            Stencil
            {
                Ref 128
                WriteMask 128
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // The block widened sideways, drawn only where the block itself is not: an edge around it.
        Pass
        {
            Name "Edge"
            Tags { "LightMode" = "UniversalForward" }

            Cull Off
            ZWrite Off
            ZTest Always

            Stencil
            {
                Ref 128
                ReadMask 128
                Comp NotEqual
            }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            static const float MinSidewaysLength = 0.001;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 position = input.positionOS.xyz;
                float2 sideways = input.normalOS.xz;
                float sidewaysLength = length(sideways);
                if (sidewaysLength > MinSidewaysLength)
                {
                    position.xz += sideways / sidewaysLength * _Width;
                }

                output.positionCS = TransformObjectToHClip(position);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
}
