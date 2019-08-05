Shader "Hidden/Water Color PostProcess/QuadPigmentManipulation"
{
    Properties
    {
        //_gControlTex("ControlTex",2D) = "black" {} // the pigmnet ctrl
        //_gColorTex("ColorTex",2D) = "black" {}
        _gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Quad Pigment Manipulation"
            //Cull Off ZWrite Off ZTest Always
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment pigmentDensityWCFrag

            //#pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadPigmentManipulation.hlsl"

            ENDHLSL
        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
