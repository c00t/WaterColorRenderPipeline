Shader "Hidden/Water Color PostProcess/CreateLinearDepth"
{
    Properties
    {
        _gDepthRange("DepthRange",Vector) = (8.0, 50.0, 0.0, 0.0)
        _gAtmosphereRange("AtmosphereRange",Vector) = (25.0,300.0,0.0,0.0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline"}

        Pass
        {
            Name "Create Linear Depth"
            ZTest Always ZWrite Off //ColorMask 0

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _DEPTH_NO_MSAA _DEPTH_MSAA_2 _DEPTH_MSAA_4

            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/Utils/CreateLinearDepthPass.hlsl"

            ENDHLSL
        }
    }
}
