Shader "SWHack/Model Selection Outline Mask"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Overlay-10"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "SelectionSilhouetteMask"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Off
            ZWrite Off
            ZTest Always
            ColorMask 0
            Stencil
            {
                Ref 128
                ReadMask 128
                WriteMask 128
                Comp Always
                Pass Replace
                Fail Keep
                ZFail Keep
            }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                return half4(0, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
