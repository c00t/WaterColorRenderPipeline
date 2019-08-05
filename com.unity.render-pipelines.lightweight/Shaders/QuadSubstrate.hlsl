#ifndef QUAD_SUBSTRATE_INCLUDED
#define QUAD_SUBSTRATE_INCLUDED
////////////////////////////////////////////////////////////////////////////////////////////////////
// quadSubstrate10.fx (HLSL)
// Brief: Substrate operations for MNPR
// Contributors: Santiago Montesdeoca, Amir Semmo
////////////////////////////////////////////////////////////////////////////////////////////////////
//              _         _             _       
//    ___ _   _| |__  ___| |_ _ __ __ _| |_ ___ 
//   / __| | | | '_ \/ __| __| '__/ _` | __/ _ \
//   \__ \ |_| | |_) \__ \ |_| | | (_| | ||  __/
//   |___/\__,_|_.__/|___/\__|_|  \__,_|\__\___|
//
////////////////////////////////////////////////////////////////////////////////////////////////////
// This shader file provides algorithms for substrate-based effects in MNPR
// 1.- Substrate lighting, adding an external lighting source to the render
// 2.- Substrate distortion, emulating the substrate fingering happening on rough substrates
////////////////////////////////////////////////////////////////////////////////////////////////////
//#include "include\\quadCommon.fxh"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/QuadCommon.hlsl"
// TEXTURES
//Texture2D gSubstrateTex;
//Texture2D gEdgeTex;
//Texture2D gControlTex;
TEXTURE2D(_gSubstrateTex);       SAMPLER(sampler_gSubstrateTex);
//TEXTURE2D(_gEdgeTex);       SAMPLER(sampler_gEdgeTex);
TEXTURE2D(_gControlTex);       SAMPLER(sampler_gControlTex);
TEXTURE2D(_gDepthTex);       SAMPLER(sampler_gDepthTex);
//Texture2D gDepthTex;
//#ifdef _DEPTH_NO_MSAA
//DEPTH_TEXTURE(_gDepthTex);//这个应该只是一个声明而已
//SAMPLER(sampler_gDepthTex);
////DEPTH_TEXTURE(_gLinearDepthTex);
////SAMPLER(sampler_gLinearDepthTex);
//#else
//DEPTH_TEXTURE_MS(_gDepthTex, MSAA_SAMPLES);
////DEPTH_TEXTURE_MS(_gLinearDepthTex, MSAA_SAMPLES);
////float4 _gZBuffer_TexelSize;
//#endif
//float4 _gDepthTex_TexelSize;

SamplerState sam_Point_Mirror_sampler;
// VARIABLES
float _gGamma;// = 1.0;
float _gSubstrateLightDir;// = 0;
float _gSubstrateLightTilt;// = 45;
float _gSubstrateShading;// = 1.0;
float _gSubstrateDistortion;

//float SampleDepth(float2 uv)
//{
//#ifdef _DEPTH_NO_MSAA
//    return SAMPLE(uv, _gDepthTex, sampler_gDepthTex);
//#else
//    int2 coord = int2(uv * _gDepthTex_TexelSize.zw);
//    float outDepth = DEPTH_DEFAULT_VALUE;
//
//    UNITY_UNROLL
//        for (int i = 0; i < MSAA_SAMPLES; ++i)
//            outDepth = DEPTH_OP(LOAD(coord, _gDepthTex, i), outDepth);
//    return outDepth;
//#endif
//}
//
//float SampleGlobalDepth(float2 uv)
//{
//#ifdef _DEPTH_NO_MSAA
//    return SAMPLE(uv, _gDepthTex, sam_Point_Mirror_sampler);
//#else
//    int2 coord = int2(uv * _gDepthTex_TexelSize.zw);
//    float outDepth = DEPTH_DEFAULT_VALUE;
//
//    UNITY_UNROLL
//        for (int i = 0; i < MSAA_SAMPLES; ++i)
//            outDepth = DEPTH_OP(LOAD(coord, _gDepthTex, i), outDepth);
//    return outDepth;
//#endif
//}

// BLENDING
float blendOverlay(in float base, in float blend) {
    return base < 0.5 ? (2.0*base*blend) : (1.0 - 2.0*(1.0 - base)*(1.0 - blend));
}

float blendLinearDodge(in float base, in float blend) {
    return base + blend;
}



//    _ _       _     _   _             
//   | (_) __ _| |__ | |_(_)_ __   __ _ 
//   | | |/ _` | '_ \| __| | '_ \ / _` |
//   | | | (_| | | | | |_| | | | | (_| |
//   |_|_|\__, |_| |_|\__|_|_| |_|\__, |
//        |___/                   |___/ 

// Contributor: Santiago Montesdeoca
// Calculates the substrate lighting on top of the rendered imagery
// -> Based on the external substrate lighting model by Montesdeoca et al. 2017
//    [2017] Edge- and substrate-based effects for watercolor stylization
half4 deferredLightingFrag(vertexOutputSampler i) : SV_Target{
    //int3 loc = int3(i.pos.xy, 0);
    float4 renderTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gColorTex, sampler_gColorTex));//gColorTex.Load(loc);
    float2 substrateNormalTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gSubstrateTex, sam_Point_Mirror_sampler)).rg - 0.5;  // bring normals to [-0.5 - 0.5]

    // get light direction
    float dirRadians = _gSubstrateLightDir * PI / 180.0;
    float3 lightDir = float3(sin(dirRadians), cos(dirRadians), (_gSubstrateLightTilt / 89.0));

    // calculate diffuse illumination
    float3 normals = float3(-substrateNormalTex, 1.0);
    float diffuse = dot(normals, lightDir);  // basic lambert
    //diffuse = 1.0 - acos(diffuse)/PI;  // angular lambert
    //diffuse = (1 + diffuse)*0.5;  // extended

    // modulate diffuse shading
    diffuse = pow(1 - diffuse, 2);  // modify curve 
    diffuse = 1 - (diffuse * _gSubstrateShading);

    // gamma correction on output
    if (_gGamma < 1) {
        // if viewport is not gamma corrected, at least keep light linearity on substrate
        diffuse = pow(abs(diffuse), 0.454545455);
    }
    renderTex.rgb *= diffuse;
    return half4(renderTex);
}

// Contributor: Amir Semmo
// Calculates the substrate lighting only on parts that have no paint applied (impasto would override any substrate structure)
// -> Extended from the external substrate lighting model by Montesdeoca et al. 2017
//    [2017] Edge- and substrate-based effects for watercolor stylization
//half4 deferredImpastoLightingFrag(vertexOutputSampler i) : SV_Target{
//    //int3 loc = int3(i.pos.xy, 0);
//    float4 renderTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gColorTex, sampler_gColorTex));//gColorTex.Load(loc);
//    float3 substrateNormalTex = float3(clamp(SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gSubstrateTex, sam_Point_Mirror_sampler)).rg - 0.5, -0.5, 0.5), 1.0); // bring normals to [-0.5 - 0.5]
//
//    // get light direction
//    float dirRadians = _gSubstrateLightDir * PI / 180.0;
//    float3 lightDir = float3(sin(dirRadians), cos(dirRadians), (_gSubstrateLightTilt / 89.0));
//
//    // calculate diffuse illumination
//    float3 normals = float3(-substrateNormalTex.xy, 1.0);
//    float diffuse = dot(normals, lightDir);  // basic lambert
//    //diffuse = 1.0 - acos(diffuse)/PI;  // angular lambert
//    //diffuse = (1 + diffuse)*0.5;  // extended lambert
//    float2 phong = saturate(float2(diffuse, pow(diffuse, _gImpastoPhongShininess) * _gImpastoPhongSpecular));  // phong based
//
//    // modulate diffuse shading
//    diffuse = pow(1 - diffuse, 2);  // modify curve 
//    diffuse = 1 - (diffuse * _gSubstrateShading);
//
//    // gamma correction on output
//    if (_gGamma < 1) {
//        // if viewport is not gamma corrected, at least keep light linearity on substrate
//        diffuse = pow(diffuse, 0.454545455);
//    }
//
//    float3 substrateColor = lerp(renderTex.rgb*diffuse, renderTex.rgb, renderTex.a);
//    float3 impastoColor = float3(blendOverlay(renderTex.r, phong.x), blendOverlay(renderTex.g, phong.x), blendOverlay(renderTex.b, phong.x)); // blend diffuse component
//           impastoColor = float3(blendLinearDodge(phong.y, impastoColor.r), blendLinearDodge(phong.y, impastoColor.g), blendLinearDodge(phong.y, impastoColor.b));  // blend specular component
//
//    // linearly blend with the alpha mask
//    renderTex.rgb = lerp(substrateColor, impastoColor, renderTex.a);
//
//    return half4(renderTex);
//}



//        _ _     _             _   _             
//     __| (_)___| |_ ___  _ __| |_(_) ___  _ __  
//    / _` | / __| __/ _ \| '__| __| |/ _ \| '_ \ 
//   | (_| | \__ \ || (_) | |  | |_| | (_) | | | |
//    \__,_|_|___/\__\___/|_|   \__|_|\___/|_| |_|
//                                                

// Contributor: Santiago Montesdeoca
// Calculates the substrate distortion generated by its roughness
// -> Based on the paper distortion approach by Montesdeoca et al. 2017
//    [2017] Art-directed watercolor stylization of 3D animations in real-time
half4 substrateDistortionFrag(vertexOutputSampler i) : SV_Target{
    //int3 loc = int3(i.pos.xy, 0); // coordinates for loading

    // get pixel values
    //half4 renderTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gColorTex, sampler_gColorTex));//_gColorTex.Load(loc);
    float2 normalTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gSubstrateTex, sampler_gSubstrateTex)).rg * 2 - 1.0;//(gSubstrateTex.Load(loc).rg * 2 - 1);  // to transform float values to -1...1
    float distortCtrl = saturate(SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gControlTex, sampler_gControlTex)).r + 0.2);  // control parameters, unpack substrate control target (y)
    float linearDepth = SAMPLE_TEXTURE2D(_gDepthTex, sampler_gDepthTex, i.uv).r;//SampleDepth(i.uv);//gDepthTex.Load(loc).r;

    // calculate uv offset
    float controlledDistortion = lerp(0, _gSubstrateDistortion, 5.0*distortCtrl);  // 0.2 is default
    float2 uvOffset = normalTex * (controlledDistortion * _gTexel);

    // check if destination is in front
    float depthDest = SAMPLE_TEXTURE2D(_gDepthTex, sam_Point_Mirror_sampler, i.uv + uvOffset).r;//SampleGlobalDepth(i.uv + uvOffset);//_gDepthTex.Sample(sam_Point_Mirror_sampler, i.uv + uvOffset).r;
    if (linearDepth - depthDest > 0.01) {
        uvOffset = float2(0.0f, 0.0f);
    }

    half4 colorDest = SampleAlbedoAlpha(i.uv + uvOffset, TEXTURE2D_PARAM(_gColorTex, sam_Point_Mirror_sampler));//gColorTex.Sample(sam_Point_Mirror_sampler, i.uv + uvOffset);
    return colorDest;
}

// Contributor: Amir Semmo
// Calculates the substrate distortion generated by its roughness only close to edges
// -> Extended from the paper distortion approach by Montesdeoca et al. 2017
//    [2017] Art-directed watercolor stylization of 3D animations in real-time
//half4 substrateDistortionEdgesFrag(vertexOutputSampler i) : SV_Target{
//    //int3 loc = int3(i.pos.xy, 0); // coordinates for loading
//
//    // get pixel values
//    float2 normalTex = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gSubstrateTex, sampler_gSubstrateTex)).rg * 2 - 1.0; //(gSubstrateTex.Load(loc).rg * 2 - 1);  // to transform float values to -1...1
//    float distortCtrl = saturate(SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gControlTex, sampler_gControlTex)).r + 0.2);//saturate(gControlTex.Load(loc).r + 0.2);  // control parameters, substrate control target (r)
//
//    // calculate uv offset
//    float controlledDistortion = lerp(0, _gSubstrateDistortion, 5.0*distortCtrl);  // 0.2 is default
//    float2 uvOffset = normalTex * (controlledDistortion * _gTexel);
//    half4 colorDest = SampleAlbedoAlpha(i.uv + uvOffset, TEXTURE2D_PARAM(_gColorTex, sam_Point_Mirror_sampler));//gColorTex.Sample(gSampler, i.uv + uvOffset);
//
//    // only distort at edges
//    half e = SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gEdgeTex, sampler_gEdgeTex)).x;//gEdgeTex.Load(int3(i.pos.xy, 0)).x;
//
//    return lerp(SampleAlbedoAlpha(i.uv, TEXTURE2D_PARAM(_gColorTex, sam_Point_Mirror_sampler)), colorDest, e);
//}



//    _            _           _                       
//   | |_ ___  ___| |__  _ __ (_) __ _ _   _  ___  ___ 
//   | __/ _ \/ __| '_ \| '_ \| |/ _` | | | |/ _ \/ __|
//   | ||  __/ (__| | | | | | | | (_| | |_| |  __/\__ \
//    \__\___|\___|_| |_|_| |_|_|\__, |\__,_|\___||___/
//                                  |_|                
// DEFERRED SUBSTRATE LIGHTING
//technique11 deferredLighting {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, deferredLightingFrag()));
//    }
//}
//technique11 deferredImpastoLighting {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, deferredImpastoLightingFrag()));
//    }
//}


// SUBSTRATE DISTORTION
// first
//technique11 substrateDistortion {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, substrateDistortionFrag()));
//    }
//}
//technique11 substrateDistortionEdges {
//    pass p0 {
//        SetVertexShader(CompileShader(vs_5_0, quadVertSampler()));
//        SetGeometryShader(NULL);
//        SetPixelShader(CompileShader(ps_5_0, substrateDistortionEdgesFrag()));
//    }
//}
#endif
