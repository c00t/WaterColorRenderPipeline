#ifndef QUAD_ADJUST_LOAD_INCLUDED
#define QUAD_ADJUST_LOAD_INCLUDED
////////////////////////////////////////////////////////////////////////////////////////////////////
// quadAdjustLoad.hlsl (HLSL)
// Brief: Adjusting and loading render targets
// Contributors: Santiago Montesdeoca, Yee Xin Chiew, Amir Semmo
////////////////////////////////////////////////////////////////////////////////////////////////////
// Unity Version Modified
// Contributors: c00t
////////////////////////////////////////////////////////////////////////////////////////////////////
//              _  _           _        _                 _
//     __ _  __| |(_)_   _ ___| |_     | | ___   __ _  __| |
//    / _` |/ _` || | | | / __| __|____| |/ _ \ / _` |/ _` |
//   | (_| | (_| || | |_| \__ \ ||_____| | (_) | (_| | (_| |
//    \__,_|\__,_|/ |\__,_|___/\__|    |_|\___/ \__,_|\__,_|
//              |__/
////////////////////////////////////////////////////////////////////////////////////////////////////
// This base shader adjusts and loads any required elements for future stylization in MNPR
////////////////////////////////////////////////////////////////////////////////////////////////////
//#include "include\\quadCommon.fxh"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/QuadCommon.hlsl"

// MAYA VARIABLES
//float gNCP : NearClipPlane;  // near clip plane distance // _ProjectionParams.y


//#ifdef _DEPTH_NO_MSAA
//DEPTH_TEXTURE(_gZBuffer);//这个应该只是一个声明而已
//SAMPLER(sampler_gZBuffer);
//DEPTH_TEXTURE(_gLinearDepthTex);
//SAMPLER(sampler_gLinearDepthTex);
//#else
//DEPTH_TEXTURE_MS(_gZBuffer, MSAA_SAMPLES);
////DEPTH_TEXTURE_MS(_gLinearDepthTex, MSAA_SAMPLES);
////DEPTH_TEXTURE_MS(_gLinearDepthTex, MSAA_SAMPLES);
//float4 _gZBuffer_TexelSize;
//float4 _gLinearDepthTex_TexelSize;
//#endif



// They Only Set Texture not the sampler
// TEXTURES
TEXTURE2D(_gDiffuseTex);       SAMPLER(sampler_gDiffuseTex);
TEXTURE2D(_gSpecularTex);       SAMPLER(sampler_gSpecularTex);
TEXTURE2D(_gSubstrateTex);       SAMPLER(sampler_gSubstrateTex);
TEXTURE2D(_gLinearDepthTex);       SAMPLER(sampler_gLinearDepthTex);
//TEXTURE2D(_gVelocityTex);       SAMPLER(sampler_gVelocityTex);
//Texture2D _gDiffuseTex;    // diffuse
//Texture2D _gSpecularTex;   // specular
//// Texture2D _gZBuffer;       // ZBuffer
//Texture2D _gSubstrateTex;  // substrate texture (paper, canvas, etc)
//Texture2D _gLinearDepthTex; // linearized depth
//Texture2D _gVelocityTex;  // velocity


// VARIABLES
// post-processing effects
float _gSaturation;
float _gContrast;
float _gBrightness;

// engine settings
float _gGamma;
//float2 _gDepthRange;// = float2(8.0, 50.0);
float3 _gSubstrateColor;// = float3(1.0, 1.0, 1.0);
//float _gEnableVelocityPV;
float _gSubstrateRoughness;
float _gSubstrateTexScale;
float2 _gSubstrateTexDimensions;
float2 _gSubstrateTexUVOffset;
float3 _gAtmosphereTint;
//float2 _gAtmosphereRange;


// MRT
struct fragmentOutput {
    half4 stylizationOutput : SV_Target0;
    half4 substrateOutput : SV_Target1;
    //float2 velocity : SV_Target3;
};



//     __                  _   _
//    / _|_   _ _ __   ___| |_(_) ___  _ __  ___
//   | |_| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
//   |  _| |_| | | | | (__| |_| | (_) | | | \__ \
//   |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
//
// remap range
float remap(float value, float oldMin, float oldMax, float newMin, float newMax) {
    return newMin + (((value - oldMin) / (oldMax - oldMin)) * (newMax - newMin));
}



//              _  _           _             _                 _
//     __ _  __| |(_)_   _ ___| |_          | | ___   __ _  __| |
//    / _` |/ _` || | | | / __| __|  _____  | |/ _ \ / _` |/ _` |
//   | (_| | (_| || | |_| \__ \ |_  |_____| | | (_) | (_| | (_| |
//    \__,_|\__,_|/ |\__,_|___/\__|         |_|\___/ \__,_|\__,_|
//              |__/

// Contributors: Santiago Montesdeoca, Yee Xin Chiew, Amir Semmo
// This shader performs four operations:
// 1.- Simple color post processing operations over the Maya render (tgt 1)
// 2.- Adds the substrate color as the background color (tgt 1)
// 3.- Loads the substrate texture into the substrate target (tgt 2)
// 4.- Modulates Unity's Z-buffer to a linear depth target with a custom range (tgt 3)

//infact they are all post-process effect
//adjustLoadMNPR

// used for multi platform
// there will be wrong, but emmmmm... I don't want to include the copypass.hlsl


// the default is to sample the camera's depth texture , so there is no need to input the depth target
// index -1 is the _gZBuffer , 1 is the _gLinearDepthTex
//float SampleDepth(float2 uv)
//{
//#ifdef _DEPTH_NO_MSAA
//    return SAMPLE(uv, _gZBuffer, sampler_gZBuffer);
//#else
//    int2 coord = int2(uv * _gZBuffer_TexelSize.zw);
//    float outDepth = DEPTH_DEFAULT_VALUE;
//
//    UNITY_UNROLL
//        for (int i = 0; i < MSAA_SAMPLES; ++i)
//            outDepth = DEPTH_OP(LOAD(coord, _gZBuffer, i), outDepth);
//    return outDepth;
//#endif
//}
//
//float SampleLinearDepth(float2 uv)
//{
//#ifdef _DEPTH_NO_MSAA
//    return SAMPLE(uv,_gLinearDepthTex, sampler_gLinearDepthTex);
//#else
//    int2 coord = int2(uv * _gLinearDepthTex_TexelSize.zw);
//    float outDepth = DEPTH_DEFAULT_VALUE;
//
//    UNITY_UNROLL
//        for (int i = 0; i < MSAA_SAMPLES; ++i)
//            outDepth = DEPTH_OP(LOAD(coord, _gLinearDepthTex, i), outDepth);
//    return outDepth;
//#endif
//}

fragmentOutput adjustLoadFrag(vertexOutputSampler i) {
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
    UNITY_SETUP_INSTANCE_ID(i);
    fragmentOutput result = (fragmentOutput) 0;
    // why there use a int3?
    //int3 loc = int3(i.pos.xy, 0);  // coordinates for loading texels

    //// LINEAR DEPTH
    //// Maya depth buffer is calculated as: zBuffer = 1 - gNCP/z;
    //float zBuffer = _gZBuffer.Load(loc).r;
    //
    //float zBuffer = SampleDepth(i.uv);//because cal uv should use the mvp matrix,all become zero
    //float depth = 1.0;
    //float depthInUnityUnits = 1000000000.0;  // maximum depth
    //float tt = 10.0;
    //if (zBuffer < 1.0) { // but the zbuffer is must below 1.0?
    //    //// zBufferParam = { (f-n)/n, 1, (f-n)/n*f, 1/f }
    //    //float LinearEyeDepth(float depth, float4 zBufferParam)
    //    //float temp = (_ProjectionParams.z - _ProjectionParams.y) / _ProjectionParams.y;
    //    depthInUnityUnits = LinearEyeDepth(zBuffer, _ZBufferParams);//- _ProjectionParams.y / (zBuffer - 1.0);  // [0 ... gNCP]
    //    depth = remap(depthInUnityUnits, _gDepthRange.x, _gDepthRange.y, 0, 1);//but this cam produce minus value??? it's just a custom depth texture
    //    //TODO: It's error ornot?
    //    // this is a equal to camera
    //    //tt = 5.0;
    //}
    //if (zBuffer > 0.5) {
    //    tt = 5.0;
    //}
    //// save depth of previous frame
    //float depthPrevious = SampleLinearDepth(i.uv).r;//SAMPLE_TEXTURE2D(_gLinearDepthTex, sampler_gLinearDepthTex, i.uv).r;//(i.uv);//_gLinearDepthTex.Load(loc).r;
    //result.depthOutput = float2(depth, depthPrevious);
    //result.depthOutput = float2(i.uv);


    // POST-PROCESSING
    // get pixel value

    //half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex));
    half4 renderTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gColorTex, sampler_gColorTex));//_gColorTex.Load(loc);

    half4 diffuseTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gDiffuseTex, sampler_gDiffuseTex));//_gDiffuseTex.Load(loc);
    //float depthOfStylizedShaders = gSpecularTex.Load(loc).a;
    half4 specMask = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gSpecularTex, sampler_gSpecularTex));//_gSpecularTex.Load(loc).a;

    if (specMask.a > 0) {
        if (_gGamma < 1) {
            // if viewport is not gamma corrected, at least keep light linearity
            diffuseTex.rgb = pow(abs(diffuseTex.rgb), 0.454545455);
        }
        //renderTex.rgb *= lightingTex.rgb;
        // shade color embedded in negative values of lightingTex
        //float3 shadeColor = lerp(saturate(lightingTex * float3(-1, -1, -1)))
        renderTex.rgb *= diffuseTex.rgb;
        // if the specular pass is disabled, then the specular pass is black.
        renderTex.rgb += specMask.rgb;//gSpecularTex.Load(loc).rgb;  // add specular contribution
    }
    // color operations
    // edit in 2019.01.24
    float luma = luminance(renderTex.rgb);
    float3 saturationResult = float3(lerp(luma.xxx, renderTex.rgb, _gSaturation));
    float3 contrastResult = lerp(float3(0.5, 0.5, 0.5), saturationResult, _gContrast * 0.5 + 0.5);
    float b = _gBrightness - 1.0;
    float3 brightnessResult = saturate(contrastResult.rgb + b);//in default it's the original render tex

    // atmospheric operations
    // Created in Create Linear Depth Pass
    //float remapedDepth = saturate(remap(depthInUnityUnits, _gAtmosphereRange.x, _gAtmosphereRange.y, 0, 1));
    float remapedDepth = SAMPLE_TEXTURE2D(_gLinearDepthTex, sampler_gLinearDepthTex, i.uv).g;
    float3 atmospericResult = lerp(brightnessResult, _gAtmosphereTint, remapedDepth);

    // add substrate color
    renderTex.rgb = lerp(_gSubstrateColor, atmospericResult, renderTex.a);
    result.stylizationOutput = renderTex;
    /*if (tt < 6.0) {
        saturationResult = float3(1, 0, 0);
    }
    result.stylizationOutput = half4(saturationResult, 1.0);*/

    // SUBSTRATE
    // readed here
    // get proper UVS
    float2 uv = i.uv * (_ScreenParams.xy / _gSubstrateTexDimensions) * (_gSubstrateTexScale)+_gSubstrateTexUVOffset;
    // get substrate pixel
    float3 substrate = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gSubstrateTex, sampler_gSubstrateTex)).rgb;//gSubstrateTex.Sample(gSampler, uv).rgb;
    substrate = substrate - 0.5;;  // bring to [-0.5 - 0 - 0.5]
    substrate *= _gSubstrateRoughness;  // define roughness
    result.substrateOutput = half4(substrate + 0.5, 0.0);  // bring back to [0 - 1]

    // velocity reset if disabled
    //result.velocity = (gEnableVelocityPV == 1.0 ? gVelocityTex.Load(loc).xy : float2(0.0, 0.0));

    return result;
}



//    _            _           _
//   | |_ ___  ___| |__  _ __ (_) __ _ _   _  ___  ___
//   | __/ _ \/ __| '_ \| '_ \| |/ _` | | | |/ _ \/ __|
//   | ||  __/ (__| | | | | | | | (_| | |_| |  __/\__ \
//    \__\___|\___|_| |_|_| |_|_|\__, |\__,_|\___||___/
//                                  |_|
// ADJUST AND LOAD EVERYTHING FOR STYLIZATION
//technique11 adjustLoadMNPR {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetPixelShader(CompileShader(ps_5_0, adjustLoadFrag()));
//    }
//}

#endif
