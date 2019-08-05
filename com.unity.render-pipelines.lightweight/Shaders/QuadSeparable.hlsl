#ifndef QUAD_SEPARABLE_INCLUDED
#define QUAD_SEPARABLE_INCLUDED
////////////////////////////////////////////////////////////////////////////////////////////////////
// quadSeparable10.fx (HLSL)
// Brief: Separable filters for watercolor stylization
// Contributors: Santiago Montesdeoca
////////////////////////////////////////////////////////////////////////////////////////////////////
//                                   _     _                        _                     _            
//    ___  ___ _ __   __ _ _ __ __ _| |__ | | ___    __      ____ _| |_ ___ _ __ ___ ___ | | ___  _ __ 
//   / __|/ _ \ '_ \ / _` | '__/ _` | '_ \| |/ _ \   \ \ /\ / / _` | __/ _ \ '__/ __/ _ \| |/ _ \| '__|
//   \__ \  __/ |_) | (_| | | | (_| | |_) | |  __/    \ V  V / (_| | ||  __/ | | (_| (_) | | (_) | |   
//   |___/\___| .__/ \__,_|_|  \__,_|_.__/|_|\___|     \_/\_/ \__,_|\__\___|_|  \___\___/|_|\___/|_|   
//            |_|                                                                                      
////////////////////////////////////////////////////////////////////////////////////////////////////
// This shader file provides separable filters to achieve the following:
// - Bleeding blur that will be blended later on to generate color bleeding
// - Extend the edges to converge edges into gaps and overlaps
////////////////////////////////////////////////////////////////////////////////////////////////////
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/QuadCommon.hlsl"//#include "..\\include\\quadCommon.fxh"

// TEXTURES
//Texture2D gEdgeTex;
//Texture2D gDepthTex;
// TEXTURES
//#ifdef _DEPTH_NO_MSAA
//DEPTH_TEXTURE(_gDepthTex);//这个应该只是一个声明而已
////SAMPLER(sampler_gDepthTex);
//#else
//DEPTH_TEXTURE_MS(_gDepthTex, MSAA_SAMPLES);
////DEPTH_TEXTURE_MS(_gLinearDepthTex, MSAA_SAMPLES);
////float4 _gDepthTex_TexelSize;
////float4 _gLinearDepthTex_TexelSize;
//#endif
//Texture2D _gDepthTex;


// don't need sampler
TEXTURE2D(_gEdgeTex);//       SAMPLER(sampler_gEdgeTex);
TEXTURE2D(_gEdgeControlTex);//       SAMPLER(sampler_gEdgeControlTex);
TEXTURE2D(_gAbstractionControlTex);//       SAMPLER(sampler_gAbstractionControlTex);
TEXTURE2D(_gDepthTex);
float4 _gDepthTex_TexelSize;
//Texture2D gEdgeControlTex;
//Texture2D gAbstractionControlTex;


// VARIABLES
float _gRenderScale;// = 1.0;
float _gBleedingThreshold;// = 0.0002;
float _gEdgeDarkeningKernel;// = 3;
float _gGapsOverlapsKernel;// = 3;
float _gBleedingRadius;// = 20;
float _gGaussianWeights[161];// max 40 bleeding radius (supersampled would be 80) // but you can't define an array in the properties


// MRT output struct
struct fragmentOutput {
    half4 bleedingBlur : SV_Target0;
    half4 darkenedEdgeBlur : SV_Target1;
};

SamplerState sampler_Point_Clamp;

//     __                  _   _                 
//    / _|_   _ _ __   ___| |_(_) ___  _ __  ___ 
//   | |_| | | | '_ \ / __| __| |/ _ \| '_ \/ __|
//   |  _| |_| | | | | (__| |_| | (_) | | | \__ \
//   |_|  \__,_|_| |_|\___|\__|_|\___/|_| |_|___/
//
// SIGMOID WEIGHT
float sigmoidWeight(float x) {
    float weight = 1.0 - x;  // inverse normalized gradient | 0...0,5...1...0,5...0

    weight = weight * 2.0 - 1.0;  // increase amplitude by 2 and shift by -1 | -1...0...1...0...-1 (so that 0,5 in the gradient is not 0
    weight = (-weight * abs(weight) * 0.5) + weight + 0.5;  // square the weights(fractions) and multiply them by 0.5 to accentuate sigmoid
    //return dot(float3(-weight, 2.0, 1.0 ),float3(abs(weight), weight, 1.0)) * 0.5;  // possibly faster version?

    return weight;
}


// COSINE WEIGHT
float cosineWeight(float x) {
    float weight = cos(x * PI / 2);
    return weight;
}


// GAUSSIAN WEIGHT
float gaussianWeight(float x, float sigma) {
    float weight = 0.15915*exp(-0.5*x*x / (sigma*sigma)) / sigma;
    //float weight = pow((6.283185*sigma*sigma), -0.5) * exp((-0.5*x*x) / (sigma*sigma));
    return weight;
}


// LINEAR WEIGHT
inline float linearWeight(float x) {
    //float weight = 1.0 - x;
    return 1.0 - x;
}



//             _                _     _            
//     ___  __| | __ _  ___    | |__ | |_   _ _ __ 
//    / _ \/ _` |/ _` |/ _ \   | '_ \| | | | | '__|
//   |  __/ (_| | (_| |  __/   | |_) | | |_| | |   
//    \___|\__,_|\__, |\___|   |_.__/|_|\__,_|_|   
//               |___/                             

// Contributors: Santiago Montesdeoca
// Extends the edges for darkened edges and gaps and overlaps
half3 edgeBlur(float2 uv, float2 dir) {

    // sample center pixel
    float3 sEdge = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gEdgeTex, sampler_Point_Clamp)).rgb;//_gEdgeTex.Sample(sampler_Point_Clamp, uv).rgb;

    // calculate darkening width
    float edgeWidthCtrl = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gEdgeControlTex, sampler_Point_Clamp)).g;//_gEdgeControlTex.Sample(sampler_Point_Clamp, uv).g;  // edge control target (g)

    float paintedWidth = lerp(0, _gEdgeDarkeningKernel * 3, edgeWidthCtrl);  // 3 times wider through local control
    int kernelRadius = max(1, (_gEdgeDarkeningKernel + paintedWidth));  // global + local control
    float normalizer = 1.0 / kernelRadius;

    /// experimental weights
    //sigmoid blur
    //float weight = sigmoidWeight(0.0);
    //cosine blur
    //float weight = cosineWeight(0.0);
    //gaussian blur
    float sigma = kernelRadius / 2.0;
    float weight = gaussianWeight(0.0, sigma);

    float darkEdgeGradient = sEdge.g * weight;
    float normDivisor = weight;

    //EDGE DARKENING GRADIENT
    //continue with lateral pixels (unroll is used to fix the dynamic loop at a certain amount)
    [unroll(100)] for (int o = 1; o < kernelRadius; o++) {
        //for (int o = 1; o < gEdgeDarkeningKernel; o++) {
        float offsetColorR = SampleAlbedoAlpha(saturate(uv + float2(o*dir)), TEXTURE2D_PARAM(_gEdgeTex, sampler_Point_Clamp)).g;//_gEdgeTex.Sample(sampler_Point_Clamp, saturate(uv + float2(o*dir))).g;
        float offsetColorL = SampleAlbedoAlpha(saturate(uv + float2(-o*dir)), TEXTURE2D_PARAM(_gEdgeTex, sampler_Point_Clamp)).g;//_gEdgeTex.Sample(sampler_Point_Clamp, saturate(uv + float2(-o * dir))).g;

        // using a sigmoid weight
        //float normGradient = (abs(o) * normalizer); //normalized gradient | 1...0,5...0...0,5...1
        //weight = sigmoidWeight(normGradient);
        // using a sinusoidal weight
        //weight = cosineWeight(normGradient);
        // using a gaussian weight
        weight = gaussianWeight(o, sigma);

        darkEdgeGradient += weight * (offsetColorL + offsetColorR);
        normDivisor += weight * 2;
    }
    darkEdgeGradient = (darkEdgeGradient / normDivisor);


    //GAPS AND OVERLAPS GRADIENT
    weight = linearWeight(0.0);
    float linearGradient = sEdge.b * weight;
    normDivisor = weight;
    normalizer = 1.0 / _gGapsOverlapsKernel;

    for (int l = 1; l < _gGapsOverlapsKernel; l++) {
        float offsetColorR = SampleAlbedoAlpha(saturate(uv + float2(l*dir)), TEXTURE2D_PARAM(_gEdgeTex, sampler_Point_Clamp)).b;//_gEdgeTex.Sample(sampler_Point_Clamp, saturate(uv + float2(l*dir))).b;
        float offsetColorL = SampleAlbedoAlpha(saturate(uv + float2(-l*dir)), TEXTURE2D_PARAM(_gEdgeTex, sampler_Point_Clamp)).b;//_gEdgeTex.Sample(sampler_Point_Clamp, saturate(uv + float2(-l * dir))).b;
        float normGradient = (l * normalizer); //normalized gradient | 1...0,5...0...0,5...1

        weight = linearWeight(normGradient);

        linearGradient += weight * (offsetColorL + offsetColorR);
        normDivisor += weight * 2;
    }

    linearGradient = linearGradient / normDivisor;

    return half3(sEdge.r, darkEdgeGradient, linearGradient);
}



//              _                _     _               _ _             
//     ___ ___ | | ___  _ __    | |__ | | ___  ___  __| (_)_ __   __ _ 
//    / __/ _ \| |/ _ \| '__|   | '_ \| |/ _ \/ _ \/ _` | | '_ \ / _` |
//   | (_| (_) | | (_) | |      | |_) | |  __/  __/ (_| | | | | | (_| |
//    \___\___/|_|\___/|_|      |_.__/|_|\___|\___|\__,_|_|_| |_|\__, |
//                                                               |___/ 

// Contributors: Santiago Montesdeoca
// Blurs certain parts of the image for color bleeding
half4 colorBleeding(float2 uv, float2 dir) {
    float4 blurPixel = float4(0.0, 0.0, 0.0, 0.0);

    // get source pixel values
    // 因为 linear depth 其实只是一个texture而已
    float sDepth = SAMPLE_TEXTURE2D(_gDepthTex, sampler_Point_Clamp, uv).r;//_gDepthTex.Sample(sampler_Point_Clamp, uv).r;//SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gDepthTex, sampler_Point_Clamp)).r;//_gDepthTex.Sample(sampler_Point_Clamp, uv).r;
    float sBlurCtrl = 0;
    if (dir.y > 0) {
        sBlurCtrl = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gColorTex, sampler_Point_Clamp)).a;//_gColorTex.Sample(sampler_Point_Clamp, uv).a;
    }
    else {
        sBlurCtrl = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gAbstractionControlTex, sampler_Point_Clamp)).b;//_gAbstractionControlTex.Sample(sampler_Point_Clamp, uv).b;  // abstraction control target (b)
    }
    float4 sColor = float4(SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_gColorTex, sampler_Point_Clamp)).rgb, sBlurCtrl);

    // go through neighborhood
    for (int a = - _gBleedingRadius; a <= _gBleedingRadius; a++) {
        float2 offsetUV = saturate(uv + float2(a*dir));

        // get destination values
        float dBlurCtrl = 0;
        if (dir.y > 0) {
            dBlurCtrl = SampleAlbedoAlpha(offsetUV, TEXTURE2D_PARAM(_gColorTex, sampler_Point_Clamp)).a;//_gColorTex.Sample(sampler_Point_Clamp, offsetUV).a;
        }
        else {
            dBlurCtrl = SampleAlbedoAlpha(offsetUV, TEXTURE2D_PARAM(_gAbstractionControlTex, sampler_Point_Clamp)).b;// _gAbstractionControlTex.Sample(sampler_Point_Clamp, offsetUV).b;  // abstraction control target (b)
        }
        float dDepth = SampleAlbedoAlpha(offsetUV, TEXTURE2D_PARAM(_gDepthTex, sampler_Point_Clamp)).r;//_gDepthTex.Sample(sampler_Point_Clamp, offsetUV).r;


        // BILATERAL DEPTH BASED BLEEDING
        float weight = _gGaussianWeights[a + _gBleedingRadius];

        float ctrlScope = max(dBlurCtrl, sBlurCtrl);
        float filterScope = abs(a) / _gBleedingRadius;
        // check if source or destination pixels are bleeded
        //if ((dBlurCtrl > 0) || (sBlurCtrl > 0)) {
        if (ctrlScope >= filterScope) {

            float bleedQ = 0;
            bool sourceBehindQ = false;
            // check if source pixel is behind
            if ((sDepth - _gBleedingThreshold) > dDepth) {
                sourceBehindQ = true;
            }

            // check bleeding cases
            if ((dBlurCtrl) && (sourceBehindQ)) {
                bleedQ = 1;
            }
            else {
                if ((sBlurCtrl) && (!sourceBehindQ)) {
                    bleedQ = 1;
                }
            }

            // bleed if necessary
            if (bleedQ) {
                float4 dColor = float4(SampleAlbedoAlpha(offsetUV, TEXTURE2D_PARAM(_gColorTex, sampler_Point_Clamp)).rgb, dBlurCtrl);//_gColorTex.Sample(sampler_Point_Clamp, offsetUV).rgb
                blurPixel += dColor * weight;  // bleed
            }
            else {
                blurPixel += sColor * weight;  // get source pixel color
            }
        }
        else {
            blurPixel += sColor * weight;
        }
    }

    return half4(blurPixel);
}



//    _                _                _        _ 
//   | |__   ___  _ __(_)_______  _ __ | |_ __ _| |
//   | '_ \ / _ \| '__| |_  / _ \| '_ \| __/ _` | |
//   | | | | (_) | |  | |/ / (_) | | | | || (_| | |
//   |_| |_|\___/|_|  |_/___\___/|_| |_|\__\__,_|_|
//                                                 
fragmentOutput horizontalFrag(vertexOutputSampler i) {
    fragmentOutput result = (fragmentOutput)0;

    float2 offset = _gTexel * float2(1.0f, 0.0f);

    // run different blurring algorithms
    result.bleedingBlur = colorBleeding(i.uv, offset);
    result.darkenedEdgeBlur = half4(edgeBlur(i.uv, offset), 0);

    //result.bleedingBlur = float4(1.0, 0, 0, 1.0);
    return result;
}



//                   _   _           _ 
//   __   _____ _ __| |_(_) ___ __ _| |
//   \ \ / / _ \ '__| __| |/ __/ _` | |
//    \ V /  __/ |  | |_| | (_| (_| | |
//     \_/ \___|_|   \__|_|\___\__,_|_|
//                                     
fragmentOutput verticalFrag(vertexOutputSampler i) {
    fragmentOutput result = (fragmentOutput)0;

    float2 offset = _gTexel * float2(0.0f, 1.0f);

    // run different blurring algorithms
    result.bleedingBlur = colorBleeding(i.uv, offset);
    result.darkenedEdgeBlur = half4(edgeBlur(i.uv, offset), result.bleedingBlur.a);
    result.darkenedEdgeBlur.b = pow(abs(result.darkenedEdgeBlur.b), 1 / result.darkenedEdgeBlur.b);  // get rid of weak gradients
    result.darkenedEdgeBlur.b = pow(result.darkenedEdgeBlur.b, 2 / _gGapsOverlapsKernel);  // adjust gamma depending on kernel size

    return result;
}



//    _            _           _                       
//   | |_ ___  ___| |__  _ __ (_) __ _ _   _  ___  ___ 
//   | __/ _ \/ __| '_ \| '_ \| |/ _` | | | |/ _ \/ __|
//   | ||  __/ (__| | | | | | | | (_| | |_| |  __/\__ \
//    \__\___|\___|_| |_|_| |_|_|\__, |\__,_|\___||___/
//                                  |_|       
// Horizontal Blur
//technique11 blurH {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, horizontalFrag()));
//    }
//}
//
//// Vertical Blur
//technique11 blurV {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, verticalFrag()));
//    }
//}
#endif
