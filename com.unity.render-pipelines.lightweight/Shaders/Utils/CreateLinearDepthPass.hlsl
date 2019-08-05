#ifndef LIGHTWEIGHT_COPY_DEPTH_PASS_INCLUDED
#define LIGHTWEIGHT_COPY_DEPTH_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

TEXTURE2D(_gLinearDepthTex);       SAMPLER(sampler_gLinearDepthTex);
float2 _gDepthRange;
float2 _gAtmosphereRange;

Varyings vert(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
    output.uv = input.uv;
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
    return output;
}

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMSArray<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_ARRAY_FLOAT(name)
#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_ARRAY_MSAA(_CameraDepthAttachment, uv, unity_StereoEyeIndex, sampleIndex)
#define SAMPLE(uv) SAMPLE_TEXTURE2D_ARRAY(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv, unity_StereoEyeIndex).r
#else
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMS<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_FLOAT(name)
#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_MSAA(_CameraDepthAttachment, uv, sampleIndex)
#define SAMPLE(uv) SAMPLE_DEPTH_TEXTURE(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv)
#endif

#ifdef _DEPTH_MSAA_2
    #define MSAA_SAMPLES 2
#elif _DEPTH_MSAA_4
    #define MSAA_SAMPLES 4
#endif

#ifdef _DEPTH_NO_MSAA
    DEPTH_TEXTURE(_CameraDepthAttachment);
    SAMPLER(sampler_CameraDepthAttachment);
#else
    DEPTH_TEXTURE_MS(_CameraDepthAttachment, MSAA_SAMPLES);
    float4 _CameraDepthAttachment_TexelSize;
#endif

#if UNITY_REVERSED_Z
    #define DEPTH_DEFAULT_VALUE 1.0
    #define DEPTH_OP min
#else
    #define DEPTH_DEFAULT_VALUE 0.0
    #define DEPTH_OP max
#endif

float SampleDepth(float2 uv)
{
#ifdef _DEPTH_NO_MSAA
    return SAMPLE(uv);
#else
    int2 coord = int2(uv * _CameraDepthAttachment_TexelSize.zw);
    float outDepth = DEPTH_DEFAULT_VALUE;

    UNITY_UNROLL
    for (int i = 0; i < MSAA_SAMPLES; ++i)
        outDepth = DEPTH_OP(LOAD(coord, i), outDepth);
    return outDepth;
#endif
}

float remap(float value, float oldMin, float oldMax, float newMin, float newMax) {
    return newMin + (((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin));
}

float2 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
    UNITY_SETUP_INSTANCE_ID(input);
    float zBuffer = SampleDepth(input.uv);
    float depth = 1.0;
    float depthInUnityUnits = 1000000000.0;  // maximum depth
    /*float tt = 10.0;*/
    //can be used
    if (zBuffer < 1.0) { // but the zbuffer is must below 1.0?
        //// zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
        //float LinearEyeDepth(float depth, float4 zBufferParam)
        //float temp = (_ProjectionParams.z - _ProjectionParams.y) / _ProjectionParams.y;
        depthInUnityUnits = LinearEyeDepth(zBuffer, _ZBufferParams);//- _ProjectionParams.y / (zBuffer - 1.0);  // [0 ... gNCP]
        depth = remap(depthInUnityUnits, _gDepthRange.x, _gDepthRange.y, 0, 1);//but this cam produce minus value??? it's just a custom depth texture
        //TODO: It's error ornot?
        // this is a equal to camera
        //tt = 5.0;
    }
    //float depthPrevious = SAMPLE_TEXTURE2D(_gLinearDepthTex, sampler_gLinearDepthTex, input.uv).r;
    float remapedDepth = saturate(remap(depthInUnityUnits, _gAtmosphereRange.x, _gAtmosphereRange.y, 0, 1));

    return float2(depth, remapedDepth);
}

#endif
