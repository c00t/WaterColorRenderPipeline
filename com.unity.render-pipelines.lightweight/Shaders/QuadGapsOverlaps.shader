Shader "Hidden/Water Color PostProcess/QuadGapsOverlaps"
{
    Properties
    {
        //_gColorTex("ColorTex",2D) = "black" {}

        //_gEdgeTex("EdgeTex",2D) = "black" {}
        //_gControlTex("ControlTex",2D) = "black" {}
        //_gBlendingTex("BlendingTex",2D) = "black" {}


        _gGORadius("GORadius",Float) = 5.0
        _gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Quad Gaps Overlaps"
            ZTest Always ZWrite On //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment gapsOverlapsFrag

            //#pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //not uuse
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadGapsOverlaps.hlsl"

            ENDHLSL
        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
