#ifndef WaterColor_LIT_PASS_INCLUDED
#define WaterColor_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/WCLighting.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"

struct Attributes
{
    float4 positionOS    : POSITION;
    float3 normalOS      : NORMAL;
    float4 tangentOS     : TANGENT;
    float2 texcoord      : TEXCOORD0;
    float2 lightmapUV    : TEXCOORD1;
    float4 controlSetA   : COLOR;//split them into three
    float2 controlSetB   : TEXCOORD2;
    float2 controlSetC   : TEXCOORD3;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};



struct Varyings
{
    float2 uv                       : TEXCOORD0; //map1 in full.fx
    DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);

    float4 posWSShininess           : TEXCOORD2;    // xyz: posWS, w: Shininess * 128

#ifdef _NORMALMAP
    half4 normal                    : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangent                   : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangent                  : TEXCOORD5;    // xyz: bitangent, w: viewDir.z , it's binormal
#else
    half3 normal                   : TEXCOORD3;
    half3 viewDir                   : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD7;
#endif
    // these value should be extract from the original vertex color
    half4 texcoordCtrlSetA : TEXCOORD8;  //0-0.25
    half4 texcoordCtrlSetB : TEXCOORD9;  //0.25-0.50
    half4 texcoordCtrlSetC : TEXCOORD10; //0.5-0.75
    // half4 velocity : TEXCOORD11;         //0.75-1.0
    //

    float4 positionCS               : SV_POSITION; //
    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

//half4 VCRemap(float min,float max,float4 VC)
//{
//    float inRange = (VC.r < max && VC.r >= min) ? 1 : 0;
//    return half4((VC * 4  - min * 4)*inRange);
//}

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData.positionWS = input.posWSShininess.xyz;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz));
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

#if SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);

    inputData.viewDirectionWS = viewDirWS;
#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);
}
void InitializeCustomData(out CustomLightInput customData)
{
    customData.diluteArea = _Dilute_Area;
    customData.cangiante = _Cangiante;
    customData.cangianteAera = _Cangiante_Area;
    customData.dilutePaint = _Dilute_Paint;
    customData.diluteColor = _Dilute_Color;
    customData.highlightRollOff = _Highlight_Roll_Off;
    customData.highlightTransparency = _Highlight_Transparency;
    customData.diffuseFactor = _Diffuse_Factor;
    customData.shadeColor = _Shade_Color;
    customData.shadeWrap = _Shade_Wrap;
}
///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Water Color (Simple Lighting) shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;

#if !SHADER_HINT_NICE_QUALITY
    viewDirWS = SafeNormalize(viewDirWS);
#endif

    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    output.uv = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.posWSShininess.xyz = vertexInput.positionWS;
    output.posWSShininess.w = _Shininess * 128.0;
    output.positionCS = vertexInput.positionCS;

#ifdef _NORMALMAP
    output.normal = half4(normalInput.normalWS, viewDirWS.x);
    output.tangent = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangent = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normal = normalInput.normalWS;
    output.viewDir = viewDirWS;
#endif

    OUTPUT_LIGHTMAP_UV(input.lightmapUV, unity_LightmapST, output.lightmapUV);
    OUTPUT_SH(output.normal.xyz, output.vertexSH);

    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif
    //0 - 0.25 -> 0 - 1.0 remap control set
    //A doesn't need unpack
    output.texcoordCtrlSetA = half4(lerp(half4(-1.0,-1.0,-1.0,-1.0), half4(1.0,1.0,1.0,1.0),input.controlSetA));
    //B.C need unpack 
    output.texcoordCtrlSetB = half4(lerp(half4(-1.0, -1.0, -1.0, -1.0), half4(1.0, 1.0, 1.0, 1.0), unpack(input.controlSetB.xy)));
    output.texcoordCtrlSetC = half4(lerp(half4(-1.0, -1.0, -1.0, -1.0), half4(1.0, 1.0, 1.0, 1.0), unpack(input.controlSetC.xy)));
    //output.velocity = VCRemap(0.75, 1.0, input.controlSet);

    return output;
}
///////////////////////////////////////////////////////////////////////////////
//                       Procedual functions                                //
///////////////////////////////////////////////////////////////////////////////
struct CtrlSetOut
{
    half4 Pigment;
    half4 Substrate;
    half4 Edge;
    half4 Abstraction;
};
struct CtrlSetIn
{
    half4 ctrlSetA;
    half4 ctrlSetB;
    half4 ctrlSetC;
    float3 posWS;
    float2 uv;
};
//TO Save Performance
half3 pigmentConstruct(half3 pigment,float2 Sam2D,float3 Sam3D)
{

#ifdef _PIG_VAR_PROCE
    //float resultVar = 0.0;
    float2 varSam2D = Sam2D * _Variation_Scale_MNPR;
    float3 varSam3D = Sam3D * _Variation_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _PIG_3D_VAR
    pigment.x = half(SimplexNoise3DFunc(varSam3D.xy, varSam3D.z, _Variation_Intensity_MNPR).noise + _Variation_Shift_MNPR + pigment.x);
#else
    pigment.x = half(SimplexNoise2DFunc(varSam2D.xy, _Variation_Intensity_MNPR).noise + _Variation_Shift_MNPR + pigment.x);
#endif
#endif

#ifdef _PIG_APP_PROCE
    //float resultVar = 0.0;
    float2 appSam2D = Sam2D * _Application_Scale_MNPR;
    float3 appSam3D = Sam3D * _Application_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _PIG_3D_APP
    pigment.y = half(SimplexNoise3DFunc(appSam3D.xy, appSam3D.z, _Application_Intensity_MNPR).noise + _Application_Shift_MNPR + pigment.y);
#else
    pigment.y = half(SimplexNoise2DFunc(appSam2D.xy, _Application_Intensity_MNPR).noise + _Application_Shift_MNPR + pigment.y);
#endif
#endif

#ifdef _PIG_DEN_PROCE
    //float resultVar = 0.0;
    float2 denSam2D = Sam2D * _Density_Scale_MNPR;
    float3 denSam3D = Sam3D * _Density_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _PIG_3D_DEN
    pigment.z = half(SimplexNoise3DFunc(denSam3D.xy, denSam3D.z, _Density_Intensity_MNPR).noise + _Density_Shift_MNPR + pigment.z);
#else
    pigment.z = half(SimplexNoise2DFunc(denSam2D.xy, _Density_Intensity_MNPR).noise + _Density_Shift_MNPR + pigment.z);
#endif
#endif
    return pigment;
}



half3 substrateConstruct(half3 substrate, float2 Sam2D, float3 Sam3D)
{

#ifdef _SUB_DIS_PROCE
    //float resultVar = 0.0;
    float2 disSam2D = Sam2D * _Distortion_Scale_MNPR;
    float3 disSam3D = Sam3D * _Distortion_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _SUB_3D_DIS
    substrate.x = half(SimplexNoise3DFunc(disSam3D.xy, disSam3D.z, _Distortion_Intensity_MNPR).noise + _Distortion_Shift_MNPR + substrate.x);
#else
    substrate.x = half(SimplexNoise2DFunc(disSam2D.xy, _Distortion_Intensity_MNPR).noise + _Distortion_Shift_MNPR + substrate.x);
#endif
#endif

#ifdef _SUB_UINC_PROCE
    //float resultVar = 0.0;
    float2 uincSam2D = Sam2D * _UIncline_Scale_MNPR;
    float3 uincSam3D = Sam3D * _UIncline_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _SUB_3D_UINC
    substrate.y = half(SimplexNoise3DFunc(uincSam3D.xy, uincSam3D.z, _UIncline_Intensity_MNPR).noise + _UIncline_Shift_MNPR + substrate.y);
#else
    substrate.y = half(SimplexNoise2DFunc(uincSam2D.xy, _UIncline_Intensity_MNPR).noise + _UIncline_Shift_MNPR + substrate.y);
#endif
#endif

#ifdef _SUB_VINC_PROCE
    //float resultVar = 0.0;
    float2 vincSam2D = Sam2D * _VIncline_Scale_MNPR;
    float3 vincSam3D = Sam3D * _VIncline_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _SUB_3D_VINC
    substrate.z = half(SimplexNoise3DFunc(vincSam3D.xy, vincSam3D.z, _VIncline_Intensity_MNPR).noise + _VIncline_Shift_MNPR + substrate.z);
#else
    substrate.z = half(SimplexNoise2DFunc(vincSam2D.xy, _VIncline_Intensity_MNPR).noise + _VIncline_Shift_MNPR + substrate.z);
#endif
#endif
    return substrate;
}

//edge
half3 edgeConstruct(half3 edge, float2 Sam2D, float3 Sam3D)
{

#ifdef _EDGE_EDGE_PROCE
    //float resultVar = 0.0;
    float2 edgeSam2D = Sam2D * _Edge_Scale_MNPR;
    float3 edgeSam3D = Sam3D * _Edge_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _EDGE_3D_EDGE
    edge.x = half(SimplexNoise3DFunc(edgeSam3D.xy, edgeSam3D.z, _Edge_Intensity_MNPR).noise + _Edge_Shift_MNPR + edge.x);
    edge.y = half(SimplexNoise3DFunc(edgeSam3D.xy, edgeSam3D.z, _Width_Intensity_MNPR).noise + _Edge_Shift_MNPR + edge.y);
#else
    edge.x = half(SimplexNoise2DFunc(edgeSam2D.xy, _Edge_Intensity_MNPR).noise + _Edge_Shift_MNPR + edge.x);
    edge.y = half(SimplexNoise2DFunc(edgeSam2D.xy, _Width_Intensity_MNPR).noise + _Edge_Shift_MNPR + edge.y);
#endif
#endif

#ifdef _EDGE_TRANS_PROCE
    //float resultVar = 0.0;
    float2 transSam2D = Sam2D * _Transition_Scale_MNPR;
    float3 transSam3D = Sam3D * _Transition_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _EDGE_3D_TRANS
    edge.z = half(SimplexNoise3DFunc(transSam3D.xy, transSam3D.z, _Transition_Intensity_MNPR).noise + _Transition_Shift_MNPR + edge.z);
#else
    edge.z = half(SimplexNoise2DFunc(transSam2D.xy, _Transition_Intensity_MNPR).noise + _Transition_Shift_MNPR + edge.z);
#endif
#endif
    return edge;
}

//abstraction
half3 abstractionConstruct(half3 abstraction, float2 Sam2D, float3 Sam3D)
{
    //detail procedual
#ifdef _ABS_DET_PROCE
    //float resultVar = 0.0;
    float2 detSam2D = Sam2D * _Detail_Scale_MNPR;
    float3 detSam3D = Sam3D * _Detail_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _ABS_3D_DET
    abstraction.x = half(SimplexNoise3DFunc(detSam3D.xy, detSam3D.z, _Detail_Intensity_MNPR).noise + _Detail_Shift_MNPR + abstraction.x);
#else
    abstraction.x = half(SimplexNoise2DFunc(detSam2D.xy, _Detail_Intensity_MNPR).noise + _Detail_Shift_MNPR + abstraction.x);
#endif
#endif

#ifdef _ABS_SHAPE_PROCE
    //float resultVar = 0.0;
    float2 shapeSam2D = Sam2D * _Shape_Scale_MNPR;
    float3 shapeSam3D = Sam3D * _Shape_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _ABS_3D_SHAPE
    abstraction.y = half(SimplexNoise3DFunc(shapeSam3D.xy, shapeSam3D.z, _Shape_Intensity_MNPR).noise + _Shape_Shift_MNPR + abstraction.y);
#else
    abstraction.y = half(SimplexNoise2DFunc(shapeSam2D.xy, _Shape_Intensity_MNPR).noise + _Shape_Shift_MNPR + abstraction.y);
#endif
#endif

#ifdef _ABS_BLEND_PROCE
    //float resultVar = 0.0;
    float2 blendSam2D = Sam2D * _Blending_Scale_MNPR;
    float3 blendSam3D = Sam3D * _Blending_Scale_MNPR;
    //_Variation_Scale_MNPR
#ifdef _ABS_3D_BLEND
    abstraction.z = half(SimplexNoise3DFunc(blendSam3D.xy, blendSam3D.z, _Blending_Intensity_MNPR).noise + _Blending_Shift_MNPR + abstraction.z);
#else
    abstraction.z = half(SimplexNoise2DFunc(blendSam2D.xy, _Blending_Intensity_MNPR).noise + _Blending_Shift_MNPR + abstraction.z);
#endif
#endif
    return abstraction;
}

/*
struct CtrlSetOut
{
    half4 Pigment;
    half4 Substrate;
    half4 Edge;
    half4 Abstraction;
};
*/
CtrlSetOut CtrlSet(half alpha, CtrlSetIn ctrlSetIn)
{
    CtrlSetOut output = (CtrlSetOut)0;
    ctrlSetIn.ctrlSetA *= alpha;
    ctrlSetIn.ctrlSetB *= alpha;
    ctrlSetIn.ctrlSetC *= alpha;
    half3 pigment = ctrlSetIn.ctrlSetA.xyz;
    half3 substrate = ctrlSetIn.ctrlSetB.xyz;
    half3 edge = ctrlSetIn.ctrlSetC.xyz;
    half3 abstraction = half3(ctrlSetIn.ctrlSetA.w, ctrlSetIn.ctrlSetB.w, ctrlSetIn.ctrlSetC.w);
    half3 defaultCtrl = half3(0, 0, 0);
    float3 Sam3D = ctrlSetIn.posWS * 0.25 * _World_Scale_MNPR;
    float2 Sam2D = ctrlSetIn.uv * _World_Scale_MNPR;
    //ctrlSetIn.ctrlSetD *= alpha;
#ifdef _VT_CTRL_P
    output.Pigment = half4(pigmentConstruct(pigment, Sam2D, Sam3D), 1.0);
#else
    output.Pigment = half4(pigmentConstruct(defaultCtrl, Sam2D, Sam3D), 1.0);
#endif
#ifdef _VT_CTRL_S
    output.Substrate = half4(substrateConstruct(substrate, Sam2D, Sam3D), 1.0);
#else
    output.Substrate = half4(substrateConstruct(defaultCtrl, Sam2D, Sam3D), 1.0);
#endif
#ifdef _VT_CTRL_E
    output.Edge = half4(edgeConstruct(edge, Sam2D, Sam3D), 1.0);
#else
    output.Edge = half4(edgeConstruct(defaultCtrl, Sam2D, Sam3D), 1.0);
#endif
#ifdef _VT_CTRL_A
    output.Abstraction = half4(abstractionConstruct(abstraction, Sam2D, Sam3D), 1.0);
#else
    output.Abstraction = half4(abstractionConstruct(defaultCtrl, Sam2D, Sam3D), 1.0);
#endif

    return output;
}



struct DiffuseFragOut
{
    half4 ColorTarget : SV_Target0; // it's the target in camera
    half4 DiffuseTarget : SV_Target1;
    half4 SpecularTarget : SV_Target2;
    half4 PigmentCtrl : SV_Target3;
    half4 SubstrateCtrl : SV_Target4;
    half4 EdgeCtrl : SV_Target5;
    half4 AbstractionCtrl : SV_Target6;
    //half4 VelocityContri : SV_Target7;
};




// Used for StandardSimpleLighting shader
DiffuseFragOut LitPassFragmentSimple(Varyings input)
{
    DiffuseFragOut output = (DiffuseFragOut)0;
    //无视
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    float2 uv = input.uv;
    half4 diffuseAlpha = SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_MainTex, sampler_MainTex));
    half3 diffuse = diffuseAlpha.rgb * _Color.rgb; //it's albedo input of painterly shading

    half alpha = Alpha(diffuseAlpha.a, _Color, _Cutoff);//diffuseAlpha.a * _Color.a;
    AlphaDiscard(alpha, _Cutoff);
#ifdef _ALPHAPREMULTIPLY_ON
    diffuse *= alpha;
#endif
#ifdef _NORMALMAP
    half3 normalTS = SampleNormal(uv, TEXTURE2D_PARAM(_BumpMap, sampler_BumpMap));
#else
    half3 normalTS = half3(0, 0, 0);
#endif

    half3 emission = SampleEmission(uv, _EmissionColor.rgb, TEXTURE2D_PARAM(_EmissionMap, sampler_EmissionMap));
    half4 specularGloss = SampleSpecularGloss(uv, diffuseAlpha.a, _SpecColor, TEXTURE2D_PARAM(_SpecGlossMap, sampler_SpecGlossMap));
    half shininess = input.posWSShininess.w;

    InputData inputData;//the data lighting function needs.
    InitializeInputData(input, normalTS, inputData);
    // the function output the 'color' other than the lighting part
    CustomLightInput CustomData;
    InitializeCustomData(CustomData);
    BlinnPhongOut blinnPhongOut = LightweightFragmentBlinnPhong(inputData, diffuse, specularGloss, shininess, emission, alpha, CustomData);
    half4 color = 0;//LightweightFragmentBlinnPhong(inputData, diffuse, specularGloss, shininess, emission, alpha, CustomData);
    
    blinnPhongOut.Color.rgb = MixFog(blinnPhongOut.Color.rgb, inputData.fogCoord);
    //return color;
    //return input.texcoordCtrlSetB; // npr
    output.ColorTarget = blinnPhongOut.Color;
    output.DiffuseTarget = blinnPhongOut.Diffuse;
    output.SpecularTarget = blinnPhongOut.Specular;

    //output.ColorTarget = blinnPhongOut.Diffuse;//half4(input.texcoordCtrlSetC);
    //output.ColorTarget = half4(uv, 1.0, 1.0);

    //Procedual Part
    
    CtrlSetIn ctrlSetIn;
    ctrlSetIn.ctrlSetA = input.texcoordCtrlSetA;
    ctrlSetIn.ctrlSetB = input.texcoordCtrlSetB;
    ctrlSetIn.ctrlSetC = input.texcoordCtrlSetC;
    ctrlSetIn.posWS = input.posWSShininess.xyz;
    ctrlSetIn.uv = input.uv;
    CtrlSetOut ctrlSetOut = CtrlSet(alpha, ctrlSetIn);
    output.PigmentCtrl = ctrlSetOut.Pigment;
    output.SubstrateCtrl = ctrlSetOut.Substrate;
    output.EdgeCtrl = ctrlSetOut.Edge;
    output.AbstractionCtrl = ctrlSetOut.Abstraction;
    //output.ColorTarget = half4(SampleAlbedoAlpha(uv, TEXTURE2D_PARAM(_CameraDepthTexture, sampler_CameraDepthTexture))); //yep,the function is designed good
    //output.VelocityContri = half4(input.velocity.xyz*alpha,1.0);



    return output;
};

#endif
