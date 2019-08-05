Shader "Hidden/Water Color PostProcess/QuadAdjustLoad"
{
    Properties
    {
        //_gZBuffer("ZBuffer",2D) = "black" {}
        //_gColorTex("ColorTex",2D) = "black" {}
        //_gLinearDepthTex("LinearDepthTex",2D) = "black" {}
        //_gDiffuseTex("DiffuseTex",2D) = "black" {}
        //_gSpecularTex("SpecularTex",2D) = "black" {}
        _gSubstrateTex("SubstrateTex",2D) = "black" {}
        //_gDiffuseTex("DiffuseTex",2D) = "black" {}
        _gSaturation("Saturation",Float) = 1.0
        _gContrast("Contrast",Float) = 1.0
        _gBrightness("Brightness",Float) = 1.0
        _gGamma("Gamma",Float) = 1.0
        _gDepthRange("DepthRange",Vector) = (8.0, 50.0, 0.0, 0.0)
        _gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)
        _gSubstrateRoughness("SubstrateRoughness",Float) = 1.0
        _gSubstrateTexScale("SubstrateTexScale",Float) = 1.0
        _gSubstrateTexDimensions("SubstrateTexDimensions",Vector) = (2048.0,2048.0,0.0,0.0)
        _gSubstrateTexUVOffset("SubstrateTexUVOffset",Vector) = (0.0,0.0,0.0,0.0)
        _gAtmosphereTint("AtmosphereTint",Color) = (1.0,1.0,1.0)
        _gAtmosphereRange("AtmosphereRange",Vector) = (25.0,300.0,0.0,0.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Quad Adjust Load"
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment adjustLoadFrag

            #pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadAdjustLoad.hlsl"

            ENDHLSL
        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
