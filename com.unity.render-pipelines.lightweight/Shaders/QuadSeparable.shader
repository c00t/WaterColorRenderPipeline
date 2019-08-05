Shader "Hidden/Water Color PostProcess/QuadSeparable"
{
    Properties
    {
        //_gEdgeTex("EdgeTex",2D) = "black" {}
        //_gColorTex("ColorTex",2D) = "black" {}
        //_gEdgeControlTex("EdgeControlTex",2D) = "black" {}
        //_gAbstractionControlTex("AbstractionControlTex",2D) = "black" {}

        _gRenderScale("RenderScale",Float) = 1.0
        _gBleedingThreshold("BleedingThreshold",Float) = 0.0002
        _gEdgeDarkeningKernel("EdgeDarkeningKernel",Float) = 3.0
        _gGapsOverlapsKernel("GapsOverlapsKernel",Float) = 3.0
        //it's a constant
        _gBleedingRadius("BleedingRadius",Float) = 10.0
        //_gSubstrateColor("SubstrateColor",Color) = (1.0, 1.0, 1.0)
        //_gSubstrateRoughness("SubstrateRoughness",Float) = 1.0
        //_gSubstrateTexScale("SubstrateTexScale",Float) = 1.0
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
            Name "Quad Separable H"
            ZTest Off ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment horizontalFrag

            #pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadSeparable.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "Quad Separable V"
            ZTest Off ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex quadVertSampler
            #pragma fragment verticalFrag

            #pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4
            //#define _DEPTH_NO_MSAA
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/QuadSeparable.hlsl"

            ENDHLSL
        }
    }
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
