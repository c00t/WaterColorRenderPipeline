////////////////////////////////////////////////////////////////////////////////////////////////////
// quadCommon.fxh (HLSL)
// Brief: Common utility shader elements for MNPR
// Contributors: Santiago Montesdeoca
////////////////////////////////////////////////////////////////////////////////////////////////////
//                          _
//     __ _ _   _  __ _  __| |       ___ ___  _ __ ___  _ __ ___   ___  _ __
//    / _` | | | |/ _` |/ _` |_____ / __/ _ \| '_ ` _ \| '_ ` _ \ / _ \| '_ \
//   | (_| | |_| | (_| | (_| |_____| (_| (_) | | | | | | | | | | | (_) | | | |
//    \__, |\__,_|\__,_|\__,_|      \___\___/|_| |_| |_|_| |_| |_|\___/|_| |_|
//       |_|
////////////////////////////////////////////////////////////////////////////////////////////////////
// This shader file provides utility variables, structs, vertex shader and functions to aid
// the development of quad operations in MNPR
////////////////////////////////////////////////////////////////////////////////////////////////////
#ifndef _QUADCOMMON_FXH
#define _QUADCOMMON_FXH

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"

// COMMON MAYA VARIABLES
// because the position in unity vertex shader is object space, so should multi a UNITY_MATRIX_MVP to achieve the same ourput
// float4x4 gWVP : WorldViewProjection;     // world-view-projection transformation // UNITY_MATRIX_VP in Unity
// float2 gScreenSize : ViewportPixelSize;  // screen size, in pixels //_ScreenParams.xy in Unity

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMSArray<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_ARRAY_FLOAT(name)
//#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_ARRAY_MSAA(_CameraDepthAttachment, uv, unity_StereoEyeIndex, sampleIndex)
#define LOAD(uv, depthTex, sampleIndex) LOAD_TEXTURE2D_ARRAY_MSAA(depthTex, uv, unity_StereoEyeIndex, sampleIndex)
//#define SAMPLE(uv) SAMPLE_TEXTURE2D_ARRAY(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv, unity_StereoEyeIndex).r
#define SAMPLE(uv,depthTex,sam_depthTex) SAMPLE_TEXTURE2D_ARRAY(depthTex, sam_depthTex, uv, unity_StereoEyeIndex).r
#else
#define DEPTH_TEXTURE_MS(name, samples) Texture2DMS<float, samples> name
#define DEPTH_TEXTURE(name) TEXTURE2D_FLOAT(name)
//#define LOAD(uv, sampleIndex) LOAD_TEXTURE2D_MSAA(_CameraDepthAttachment, uv, sampleIndex)
#define LOAD(uv, depthTex, sampleIndex) LOAD_TEXTURE2D_MSAA(depthTex, uv, sampleIndex)
//#define SAMPLE(uv) SAMPLE_DEPTH_TEXTURE(_CameraDepthAttachment, sampler_CameraDepthAttachment, uv)
#define SAMPLE(uv,depthTex,sam_depthTex) SAMPLE_DEPTH_TEXTURE(depthTex,sam_depthTex, uv) //use the same sampler
#endif

#ifdef _DEPTH_MSAA_2
#define MSAA_SAMPLES 2
#elif _DEPTH_MSAA_4
#define MSAA_SAMPLES 4
#endif

#if UNITY_REVERSED_Z
#define DEPTH_DEFAULT_VALUE 1.0
#define DEPTH_OP min
#else
#define DEPTH_DEFAULT_VALUE 0.0
#define DEPTH_OP max
#endif

// COMMON VARIABLES
// the init should be
//In Compute shaders (hlsl), the initialization only works when the variable is marked as static. When you do that, the variable also becomes invisible to the application
static half3 luminanceCoeff = float3(0.241, 0.691, 0.068);
static float2 _gTexel = 1.0f / _ScreenParams.xy;
//static const float PI = 3.14159265f; //already defined


// COMMON TEXTURES
//Texture2D gColorTex;      // color target
TEXTURE2D(_gColorTex);       SAMPLER(sampler_gColorTex);

// COMMON SAMPLERS
//uniform SamplerState gSampler;
//TEXTURE2D(_gSampler);       SAMPLER(sampler_gSampler);


//        _                   _
//    ___| |_ _ __ _   _  ___| |_ ___
//   / __| __| '__| | | |/ __| __/ __|
//   \__ \ |_| |  | |_| | (__| |_\__ \
//   |___/\__|_|   \__,_|\___|\__|___/
//
// base input structs
// but in unity, the position is in object space
struct appData {
    float4 vertex : POSITION;//it's Object-Space
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct appDataSampler {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct vertexOutput {
    float4 pos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};
struct vertexOutputSampler {
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};



//                   _                    _               _
//   __   _____ _ __| |_ _____  __    ___| |__   __ _  __| | ___ _ __ ___
//   \ \ / / _ \ '__| __/ _ \ \/ /   / __| '_ \ / _` |/ _` |/ _ \ '__/ __|
//    \ V /  __/ |  | ||  __/>  <    \__ \ | | | (_| | (_| |  __/ |  \__ \
//     \_/ \___|_|   \__\___/_/\_\   |___/_| |_|\__,_|\__,_|\___|_|  |___/
//
// VERTEX SHADER
vertexOutput quadVert(appData v) {
    vertexOutput o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = TransformObjectToHClip(v.vertex.xyz);//mul(float4(v.vertex, 1.0f), UNITY_MATRIX_MVP);//把vertex position World position // Equal to M * VP Matrix
    return o;
}

// VERTEX SHADER (with uvs)
vertexOutputSampler quadVertSampler(appDataSampler v) {
    vertexOutputSampler o;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
    o.pos = TransformObjectToHClip(v.vertex.xyz);//mul(float4(v.vertex, 1.0f), UNITY_MATRIX_MVP);
    o.uv = v.texcoord; // but what it means?
    return o;
}



//     __                  _   _
//    / _|_   _ _ __   ___| |_(_) ___  _ __  ___
//   | |_| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
//   |  _| |_| | | | | (__| |_| | (_) | | | \__ \
//   |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
//
float luminance(half3 color) {
    return dot(color.rgb, luminanceCoeff);
}

float4 unpremultiply(float4 color) {
    if (color.a) {
        color.rgb /= color.a;
    }
    return color;
}

#endif /* _QUADCOMMON_FXH */
