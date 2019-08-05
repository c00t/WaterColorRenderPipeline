#ifndef WATERCOLOR_LIT_INPUT_INCLUDED
#define WATERCOLOR_LIT_INPUT_INCLUDED

#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.lightweight/ShaderLibrary/SurfaceInput.hlsl"

CBUFFER_START(UnityPerMaterial)
float4 _MainTex_ST;
half4 _Color;
half4 _SpecColor;
half4 _EmissionColor;
half _Cutoff;
half _Shininess;

//these param should never be 
//Painterly Shading
//half3 _Color_Tint;
half _Cangiante;
half _Cangiante_Area;

half3 _Dilute_Color;
half _Dilute_Area;
half _Dilute_Paint;

half3 _Shade_Color;
half _Shade_Wrap;

half _Diffuse_Factor;

half _Highlight_Roll_Off;
half _Highlight_Transparency;
//Stylization ( procedural )

#ifdef _PIG_VAR_PROCE
half _Variation_Scale_MNPR;
half _Variation_Intensity_MNPR;
half _Variation_Shift_MNPR;
#endif

half _World_Scale_MNPR;

#ifdef _PIG_APP_PROCE
half _Application_Scale_MNPR;
half _Application_Intensity_MNPR;
half _Application_Shift_MNPR;
#endif

#ifdef _PIG_DEN_PROCE
half _Density_Scale_MNPR;
half _Density_Intensity_MNPR;
half _Density_Shift_MNPR;
#endif

#ifdef _SUB_DIS_PROCE
half _Distortion_Scale_MNPR;
half _Distortion_Intensity_MNPR;
half _Distortion_Shift_MNPR;
#endif

#ifdef _SUB_UINC_PROCE
half _UIncline_Scale_MNPR;
half _UIncline_Intensity_MNPR;
half _UIncline_Shift_MNPR;
#endif

#ifdef _SUB_VINC_PROCE
half _VIncline_Scale_MNPR;
half _VIncline_Intensity_MNPR;
half _VIncline_Shift_MNPR;
#endif

#ifdef _EDGE_EDGE_PROCE
half _Edge_Scale_MNPR;
half _Edge_Intensity_MNPR;
half _Edge_Shift_MNPR;
half _Width_Intensity_MNPR;
#endif

#ifdef _EDGE_TRANS_PROCE
half _Transition_Scale_MNPR;
half _Transition_Intensity_MNPR;
half _Transition_Shift_MNPR;
#endif

#ifdef _ABS_DET_PROCE
half _Detail_Scale_MNPR;
half _Detail_Intensity_MNPR;
half _Detail_Shift_MNPR;
#endif

#ifdef _ABS_SHAPE_PROCE
half _Shape_Scale_MNPR;
half _Shape_Intensity_MNPR;
half _Shape_Shift_MNPR;
#endif

#ifdef _ABS_BLEND_PROCE
half _Blending_Scale_MNPR;
half _Blending_Intensity_MNPR;
half _Blending_Shift_MNPR;
#endif

#ifdef _WCSPECULAR // these keyword can be enable in c# code 
half _Horizontal_Roll_Off;
half _Vertical_Roll_Off;
half _Specular_Diffusion;
half _Specular_Transparency;
#endif
CBUFFER_END

TEXTURE2D(_SpecGlossMap);       SAMPLER(sampler_SpecGlossMap);

half4 SampleSpecularGloss(half2 uv, half alpha, half4 specColor, TEXTURE2D_ARGS(specGlossMap, sampler_specGlossMap))
{
    half4 specularGloss = half4(0.0h, 0.0h, 0.0h, 1.0h);
#ifdef _SPECGLOSSMAP
    specularGloss = SAMPLE_TEXTURE2D(specGlossMap, sampler_specGlossMap, uv);
#elif defined(_SPECULAR_COLOR)
    specularGloss = specColor;
#endif

#ifdef _GLOSSINESS_FROM_BASE_ALPHA
    specularGloss.a = alpha;
#endif
    return specularGloss;
}

#endif
