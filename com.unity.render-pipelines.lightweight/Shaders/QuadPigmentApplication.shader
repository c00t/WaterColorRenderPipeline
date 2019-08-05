Shader "Hidden/Water Color PostProcess/QuadPigmentApplication"
{
    Properties
    {
        //_gZBuffer("ZBuffer",2D) = "black" {}

        //_gColorTex("ColorTex",2D) = "black" {}
        //_gFilterTex("FilterTex",2D) = "black" {}
        //_gControlTex("ControlTex",2D) = "black" {}
        //_gSpecularTex("SpecularTex",2D) = "black" {}
        /*_gSubstrateTex("SubstrateTex",2D) = "black" {}*/
        //_gDiffuseTex("DiffuseTex",2D) = "black" {}
        _gPigmentDensity("PigmentDensity",Float) = 5.0
        _gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)
        _gDryBrushThreshold("_gDryBrushThreshold",Float) =5.0 // TODO: edit it
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Quad Pigment Application"
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment pigmentApplicationWCFrag

            //#pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadPigmentApplication.hlsl"

            ENDHLSL
        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
