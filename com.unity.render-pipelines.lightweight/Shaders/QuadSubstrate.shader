Shader "Hidden/Water Color PostProcess/QuadSubstrate"
{
    Properties
    {
        /*_gZBuffer("ZBuffer",2D) = "black" {}*/
        //_gColorTex("ColorTex",2D) = "black" {}
        ////_gLinearDepthTex("LinearDepthTex",2D) = "black" {}
        //_gControlTex("ControlTex",2D) = "black" {}
        //_gEdgeTex("EdgeTex",2D) = "black" {}
        //_gSubstrateTex("SubstrateTex",2D) = "black" {}
        //_gDepthTex("DepthTex",2D) = "black" {}

        _gSubstrateShading("SubstrateShading",Float) = 0
        _gSubstrateLightTilt("SubstrateLightTilt",Float) = 89
        _gSubstrateLightDir("SubstrateLightDir",Float) = 180
        _gGamma("Gamma",Float) = 1.0
        _gSubstrateDistortion("SubstrateDistortion",Float) = 1.0

        //_gDepthRange("DepthRange",Vector) = (8.0, 50.0, 0.0, 0.0)
        //_gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)
        //
        //
        //_gSubstrateTexDimensions("SubstrateTexDimensions",Vector) = (1.0,1.0,0.0,0.0)
        //_gSubstrateTexUVOffset("SubstrateTexUVOffset",Vector) = (0.0,0.0,0.0,0.0)
        //_gAtmosphereTint("AtmosphereTint",Color) = (1.0,1.0,1.0)
        //_gAtmosphereRange("AtmosphereRange",Vector) = (25.0,300.0,0.0,0.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Quad Substrate Distortion"
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment substrateDistortionFrag

            #pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadSubstrate.hlsl"

            ENDHLSL
        }
        Pass
        {
            Name "Quad Substrate Lighting"
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment deferredLightingFrag

            //#pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            #define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadSubstrate.hlsl"
            ENDHLSL

        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
