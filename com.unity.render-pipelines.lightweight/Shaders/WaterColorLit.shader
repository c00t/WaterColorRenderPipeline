// Shader targeted for low end devices. Single Pass Forward Rendering.
Shader "Lightweight Render Pipeline/Water Color Local"
{
    // Keep properties of StandardSpecular shader for upgrade reasons.
    Properties

    {
        _Color("Color", Color) = (0.5, 0.5, 0.5, 1)
        _MainTex("Base (RGB) Glossiness / Alpha (A)", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5 // it's surface_Mask_Cutoff

        _Shininess("Shininess", Range(0.01, 1.0)) = 0.5
        _GlossMapScale("Smoothness Factor", Range(0.0, 1.0)) = 1.0

        //WaterColor Specific they are all in the cbuffer, but some of them will be pull out
        _Cangiante("Cangiante",Range(0.0,1.0)) = 0.2
        _Cangiante_Area("Cangiante Area",Range(0.0,1.0)) = 1.0


        _Dilute_Color("Dilute Color",Color) = (1.0,1.0,1.0)
        _Dilute_Area("Dilute Aera",Range(0.0,1.0)) = 1.0
        _Dilute_Paint("Dilute Paint",Range(0.0,1.0)) = 0.8

        _Shade_Color("Shade Color",Color) = (0.0,0.0,0.0)
        _Shade_Wrap("Shade Wrap",Range(0.0,1.0)) = 0.0

        _Diffuse_Factor("Diffuse Factor",Range(0.0,1.0)) = 0.2

        _Highlight_Roll_Off("Highlight Roll Off",Range(0.0,1.0)) = 0.0
        _Highlight_Transparency("Highlight Transparency",Range(0.0,99.0)) = 0.0
        //_Color_Tint("Color Tint")
        //Procedual Process
        _Variation_Scale_MNPR("Variation Scale",Range(0.0,99.0)) = 1.0
        _Variation_Intensity_MNPR("Variation Intensity",Range(0.0,99.0)) = 10.0
        _Variation_Shift_MNPR("Variation Shift",Range(-1.0,1.0)) = 0.0

        _World_Scale_MNPR("World Scale",Range(0.0,99.0)) = 1.0
        _Application_Scale_MNPR("Application Scale",Range(0.0,99.0)) = 1.31
        _Application_Intensity_MNPR("Application Intensity",Range(0.0,99.0)) = 10.0
        _Application_Shift_MNPR("Application Shift",Range(-1.0,1.0)) = 0.0
        
        _Density_Scale_MNPR("Density Scale",Range(0.0,99.0)) = 1.22
        _Density_Intensity_MNPR("Density Intensity",Range(0.0,99.0)) = 10.0
        _Density_Shift_MNPR("Density Shift",Range(-1.0,1.0)) = 0.0

        _Distortion_Scale_MNPR("Distortion Scale",Range(0.0,99.0)) = 1.0
        _Distortion_Intensity_MNPR("Distortion Intensity",Range(0.0,99.0)) = 10.0
        _Distortion_Shift_MNPR("Distortion Shift",Range(-1.0,1.0)) = 0.0

        _UIncline_Scale_MNPR("UIncline Scale",Range(0.0,99.0)) = 1.0
        _UIncline_Intensity_MNPR("UIncline Intensity",Range(0.0,99.0)) = 10.0
        _UIncline_Shift_MNPR("UIncline Shift",Range(-1.0,1.0)) = 0.0

        _VIncline_Scale_MNPR("VIncline Scale",Range(0.0,99.0)) = 1.0
        _VIncline_Intensity_MNPR("VIncline Intensity",Range(0.0,99.0)) = 10.0
        _VIncline_Shift_MNPR("VIncline Shift",Range(-1.0,1.0)) = 0.0

        _Edge_Scale_MNPR("Edge Scale",Range(0.0,99.0)) = 1.0
        _Edge_Intensity_MNPR("Edge Intensity",Range(0.0,99.0)) = 10.0
        _Edge_Shift_MNPR("Edge Shift",Range(-1.0,1.0)) = 0.0
        _Width_Intensity_MNPR("Width Intensity",Range(0.0,99.0)) = 10.0

        _Transition_Scale_MNPR("Transition Scale",Range(0.0,99.0)) = 1.0
        _Transition_Intensity_MNPR("Transition Intensity",Range(0.0,99.0)) = 10.0
        _Transition_Shift_MNPR("Transition Shift",Range(-1.0,1.0)) = 0.0

        _Shape_Scale_MNPR("Shape Scale",Range(0.0,99.0)) = 1.0
        _Shape_Intensity_MNPR("Shape Intensity",Range(0.0,99.0)) = 10.0
        _Shape_Shift_MNPR("Shape Shift",Range(-1.0,1.0)) = 0.0

        _Detail_Scale_MNPR("Detail Scale",Range(0.0,99.0)) = 1.0
        _Detail_Intensity_MNPR("Detail Intensity",Range(0.0,99.0)) = 10.0
        _Detail_Shift_MNPR("Detail Shift",Range(-1.0,1.0)) = 0.0

        _Blending_Scale_MNPR("Blending Scale",Range(0.0,99.0)) = 1.0
        _Blending_Intensity_MNPR("Blending Intensity",Range(0.0,99.0)) = 10.0
        _Blending_Shift_MNPR("Blending Shift",Range(-1.0,1.0)) = 0.0
        //Used by Specular Pass, to be honest, the 'specular' effect is strange
        _Horizontal_Roll_Off("Horizontal Roll Off(not used)",Range(0.0,1.0)) = 0.341463
        _Vertical_Roll_Off("Vertical Roll Off(not used)",Range(0.0,1.0)) = 0.053733
        _Specular_Diffusion("Specular Diffusion(not used)",Range(0.0,99.0)) = 0.0
        _Specular_Transparency("Specular Transparency(not used)",Range(0.0,1.0)) = 0.0
        //
        //_Test("test",Color) = (1.0,1.0,1.0)


        _Glossiness("Glossiness", Range(0.0, 1.0)) = 0.5
        [Enum(Specular Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel("Smoothness texture channel", Float) = 0

        [HideInInspector] _SpecSource("Specular Color Source", Float) = 0.0
        _SpecColor("Specular", Color) = (0.5, 0.5, 0.5)
        _SpecGlossMap("Specular", 2D) = "white" {}
        [HideInInspector] _GlossinessSource("Glossiness Source", Float) = 0.0
        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0


        [HideInInspector] _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {} // they called normal map as bump map

        _EmissionColor("Emission Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        //_AlphaMap("Alpha",2D) = "white" {}

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        _Cull("Cull Mode(0:double,1:back,2:front)", Float) = 2.0

        [ToogleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
        [Toggle(_ALPHATEST_ON)] _ALPHATEST_ON("Alpha Test", Int) = 0
        [Toggle(_NORMALMAP)] _NORMALMAP("Use Normal Map", Int) = 0
        [Toggle(_VT_CTRL_P)] _VT_CTRL_P("Pigment VTCTRL", Int) = 0
        [Toggle(_PIG_VAR_PROCE)] _PIG_VAR_PROCE("Pigment Var Proce", Int) = 0
        [Toggle(_PIG_3D_VAR)] _PIG_3D_VAR("Pigment Var 3D", Int) = 0
        [Toggle(_PIG_APP_PROCE)] _PIG_APP_PROCE("Pigment App Proce", Int) = 0
        [Toggle(_PIG_3D_APP)] _PIG_3D_APP("Pigment App 3D", Int) = 0
        [Toggle(_PIG_DEN_PROCE)] _PIG_DEN_PROCE("Pigment Den Proce", Int) = 0
        [Toggle(_PIG_3D_DEN)] _PIG_3D_DEN("Pigment Den 3D", Int) = 0
        
        [Toggle(_VT_CTRL_S)] _VT_CTRL_S("Substrate VTCTRL", Int) = 0
        [Toggle(_SUB_DIS_PROCE)] _SUB_DIS_PROCE("Substrate Dis Proce", Int) = 0
        [Toggle(_SUB_3D_DIS)] _SUB_3D_DIS("Substrate Dis 3D", Int) = 0
        [Toggle(_SUB_UINC_PROCE)] _SUB_UINC_PROCE("Substrate Uinc Proce", Int) = 0
        [Toggle(_SUB_3D_UINC)] _SUB_3D_UINC("Substrate Uinc 3D", Int) = 0
        [Toggle(_SUB_VINC_PROCE)] _SUB_VINC_PROCE("Substrate Vinc Proce", Int) = 0
        [Toggle(_SUB_3D_VINC)] _SUB_3D_VINC("Substrate Vinc 3D", Int) = 0

        [Toggle(_VT_CTRL_E)] _VT_CTRL_E("Edge VTCTRL", Int) = 0
        [Toggle(_EDGE_EDGE_PROCE)] _EDGE_EDGE_PROCE("Edge Edge Proce", Int) = 0
        [Toggle(_EDGE_3D_EDGE)] _EDGE_3D_EDGE("Edge Edge 3D", Int) = 0
        [Toggle(_EDGE_TRANS_PROCE)] _EDGE_TRANS_PROCE("Edge Trans Proce", Int) = 0
        [Toggle(_EDGE_3D_TRANS)] _EDGE_3D_TRANS("Edge Trans 3D", Int) = 0

        [Toggle(_VT_CTRL_A)] _VT_CTRL_A("Abstraction VTCTRL", Int) = 0
        [Toggle(_ABS_DET_PROCE)] _ABS_DET_PROCE("Abstraction Det Proce", Int) = 0
        [Toggle(_ABS_3D_DET)] _ABS_3D_DET("Abstraction Det 3D", Int) = 0
        [Toggle(_ABS_SHAPE_PROCE)] _ABS_SHAPE_PROCE("Abstraction Shape Proce", Int) = 0
        [Toggle(_ABS_3D_SHAPE)] _ABS_3D_SHAPE("Abstraction Shape 3D", Int) = 0
        [Toggle(_ABS_BLEND_PROCE)] _ABS_BLEND_PROCE("Abstraction Blend Proce", Int) = 0
        [Toggle(_ABS_3D_BLEND)] _ABS_3D_BLEND("Abstraction Blend 3D", Int) = 0
    }
    //HLSLINCLUDE

    //ENDHLSL
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "LightweightPipeline" "IgnoreProjector" = "True"}
        LOD 300
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "LightweightForward" }

            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _ _SPECGLOSSMAP _SPECULAR_COLOR
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _EMISSION
            #pragma shader_feature _RECEIVE_SHADOWS_OFF

            #pragma shader_feature _VT_CTRL_P
            #pragma shader_feature _PIG_VAR_PROCE
            #pragma shader_feature _PIG_3D_VAR
            #pragma shader_feature _PIG_APP_PROCE
            #pragma shader_feature _PIG_3D_APP
            #pragma shader_feature _PIG_DEN_PROCE
            #pragma shader_feature _PIG_3D_DEN

            #pragma shader_feature _VT_CTRL_S
            #pragma shader_feature _SUB_DIS_PROCE
            #pragma shader_feature _SUB_3D_DIS
            #pragma shader_feature _SUB_UINC_PROCE
            #pragma shader_feature _SUB_3D_UINC
            #pragma shader_feature _SUB_VINC_PROCE
            #pragma shader_feature _SUB_3D_VINC

            #pragma shader_feature _VT_CTRL_E
            #pragma shader_feature _EDGE_EDGE_PROCE
            #pragma shader_feature _EDGE_3D_EDGE
            #pragma shader_feature _EDGE_TRANS_PROCE
            #pragma shader_feature _EDGE_3D_TRANS
            #pragma shader_feature _VT_CTRL_A
            #pragma shader_feature _ABS_DET_PROCE
            #pragma shader_feature _ABS_3D_DET
            #pragma shader_feature _ABS_SHAPE_PROCE
            #pragma shader_feature _ABS_3D_SHAPE
            #pragma shader_feature _ABS_BLEND_PROCE
            #pragma shader_feature _ABS_3D_BLEND

            // -------------------------------------
            // Lightweight Pipeline keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile_fog

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            #define BUMP_SCALE_NOT_SUPPORTED 1
            //#define _VT_CTRL_P
            //#define _PIG_VAR_PROCE
            //#define _PIG_APP_PROCE
            //#define _PIG_DEN_PROCE
            //#define _SUB_DIS_PROCE
            //#define _SUB_UINC_PROCE
            //#define _SUB_VINC_PROCE
            //#define _EDGE_EDGE_PROCE
            //#define _EDGE_TRANS_PROCE
            //#define _ABS_SHAPE_PROCE
            //#define _ABS_BLEND_PROCE
            //#define _ABS_DET_PROCE
            //#define _ALPHATEST_ON
            #include "WaterColorLitInput.hlsl"
            #include "WaterColorLitForwardPass.hlsl"
            ENDHLSL
        }
        //emmm currently don't write it, it's equal to Set Specualr to False MACRO '_WCSPECULAR'
        //Pass
        //{
        //    Name "Addition Specular"

        //}
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "WaterColorLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            #pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "WaterColorLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }

        // This pass it not used during regular rendering, only for lightmap baking.
        Pass
        {
            Name "Meta"
            Tags{ "LightMode" = "Meta" }

            Cull Off

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex LightweightVertexMeta
            #pragma fragment LightweightFragmentMetaSimple

            #pragma shader_feature _EMISSION
            #pragma shader_feature _SPECGLOSSMAP

            #include "WaterColorLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.lightweight/Shaders/SimpleLitMetaPass.hlsl"

            ENDHLSL
        }
    }
    Fallback "Hidden/InternalErrorShader"
    //CustomEditor "UnityEditor.Rendering.LWRP.WaterColorLitShaderGUI"
}
