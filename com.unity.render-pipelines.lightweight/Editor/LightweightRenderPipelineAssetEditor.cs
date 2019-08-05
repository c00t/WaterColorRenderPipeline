using UnityEditor;
using UnityEditor.Experimental.Rendering;
using UnityEditor.Rendering;

namespace UnityEngine.Rendering.LWRP
{
    [CustomEditor(typeof(LightweightRenderPipelineAsset))]
    public class LightweightRenderPipelineAssetEditor : Editor
    {
        internal class Styles
        {
            // Groups
            public static GUIContent generalSettingsText = EditorGUIUtility.TrTextContent("General");
            public static GUIContent qualitySettingsText = EditorGUIUtility.TrTextContent("Quality");
            public static GUIContent lightingSettingsText = EditorGUIUtility.TrTextContent("Lighting");
            public static GUIContent shadowSettingsText = EditorGUIUtility.TrTextContent("Shadows");
            public static GUIContent advancedSettingsText = EditorGUIUtility.TrTextContent("Advanced");
            public static GUIContent watercolorSettingsText = EditorGUIUtility.TrTextContent("Water Color Settings");

            //Water Color
            public static GUIContent WaterColorEnableText = EditorGUIUtility.TrTextContent("Water Color","Enable WaterColor Passes.");


            // General
            public static GUIContent requireDepthTextureText = EditorGUIUtility.TrTextContent("Depth Texture", "If enabled the pipeline will generate camera's depth that can be bound in shaders as _CameraDepthTexture.");
            public static GUIContent requireOpaqueTextureText = EditorGUIUtility.TrTextContent("Opaque Texture", "If enabled the pipeline will copy the screen to texture after opaque objects are drawn. For transparent objects this can be bound in shaders as _CameraOpaqueTexture.");
            public static GUIContent opaqueDownsamplingText = EditorGUIUtility.TrTextContent("Opaque Downsampling", "The downsampling method that is used for the opaque texture");

            // Quality
            public static GUIContent hdrText = EditorGUIUtility.TrTextContent("HDR", "Controls the global HDR settings.");
            public static GUIContent msaaText = EditorGUIUtility.TrTextContent("Anti Aliasing (MSAA)", "Controls the global anti aliasing settings.");
            public static GUIContent renderScaleText = EditorGUIUtility.TrTextContent("Render Scale", "Scales the camera render target allowing the game to render at a resolution different than native resolution. UI is always rendered at native resolution. When VR is enabled, this is overridden by XRSettings.");

            // Main light
            public static GUIContent mainLightRenderingModeText = EditorGUIUtility.TrTextContent("Main Light", "Main light is the brightest directional light.");
            public static GUIContent supportsMainLightShadowsText = EditorGUIUtility.TrTextContent("Cast Shadows", "If enabled the main light can be a shadow casting light.");
            public static GUIContent mainLightShadowmapResolutionText = EditorGUIUtility.TrTextContent("Shadow Resolution", "Resolution of the main light shadowmap texture. If cascades are enabled, cascades will be packed into an atlas and this setting controls the maximum shadows atlas resolution.");

            // Additional lights
            public static GUIContent addditionalLightsRenderingModeText = EditorGUIUtility.TrTextContent("Additional Lights", "Additional lights support.");
            public static GUIContent perObjectLimit = EditorGUIUtility.TrTextContent("Per Object Limit", "Maximum amount of additional lights. These lights are sorted and culled per-object.");
            public static GUIContent supportsAdditionalShadowsText = EditorGUIUtility.TrTextContent("Cast Shadows", "If enabled shadows will be supported for spot lights.\n");
            public static GUIContent additionalLightsShadowmapResolution = EditorGUIUtility.TrTextContent("Shadow Resolution", "All additional lights are packed into a single shadowmap atlas. This setting controls the atlas size.");

            // Shadow settings
            public static GUIContent shadowDistanceText = EditorGUIUtility.TrTextContent("Distance", "Maximum shadow rendering distance.");
            public static GUIContent shadowCascadesText = EditorGUIUtility.TrTextContent("Cascades", "Number of cascade splits used in for directional shadows");
            public static GUIContent shadowDepthBias = EditorGUIUtility.TrTextContent("Depth Bias", "Controls the distance at which the shadows will be pushed away from the light. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent shadowNormalBias = EditorGUIUtility.TrTextContent("Normal Bias", "Controls distance at which the shadow casting surfaces will be shrunk along the surface normal. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent supportsSoftShadows = EditorGUIUtility.TrTextContent("Soft Shadows", "If enabled pipeline will perform shadow filtering. Otherwise all lights that cast shadows will fallback to perform a single shadow sample.");

            // Advanced settings
            public static GUIContent srpBatcher = EditorGUIUtility.TrTextContent("SRP Batcher (Experimental)", "If enabled, the render pipeline uses the SRP batcher.");
            public static GUIContent dynamicBatching = EditorGUIUtility.TrTextContent("Dynamic Batching", "If enabled, the render pipeline will batch drawcalls with few triangles together by copying their vertex buffers into a shared buffer on a per-frame basis.");
            public static GUIContent mixedLightingSupportLabel = EditorGUIUtility.TrTextContent("Mixed Lighting", "Support for mixed light mode.");

            // WaterColor settings
            public static GUIContent watercolorEngineSettings = EditorGUIUtility.TrTextContent("Engine Settings", "Some engine settings.");
            public static GUIContent depthRange = EditorGUIUtility.TrTextContent("Depth Range", "Range of linear depth target(min, max).");
            public static GUIContent atmosphereTint = EditorGUIUtility.TrTextContent("Atmosphere Tint", "Atmosphere tint color at distance.");
            public static GUIContent atmosphereRange = EditorGUIUtility.TrTextContent("Atmosphere Range", "Atmosphere Range at which the tint color will take effects(min, max).");

            public static GUIContent watercolorSettngs = EditorGUIUtility.TrTextContent("Watercolor Settings", "Watercolor settings such as substrate, edge or pigment");
            public static GUIContent pigmentDensity = EditorGUIUtility.TrTextContent("Pigment Density", "Set the pigment density.");
            public static GUIContent edgeDarkenIntensity = EditorGUIUtility.TrTextContent("Edge Darken Intensity", "Set the edge darken intensity.");
            public static GUIContent edgeDarkenWidth = EditorGUIUtility.TrTextContent("Edge Darken Width", "Set the edge darken width.");
            public static GUIContent bleedingRadius = EditorGUIUtility.TrTextContent("Bleeding Radius", "Set the bleeding radius.");
            public static GUIContent bleedingThreshold = EditorGUIUtility.TrTextContent("Bleeding Threshold", "Set the bleeding threshold.");
            public static GUIContent drybrushThreshold = EditorGUIUtility.TrTextContent("Drybrush Threshold", "Set the drybrush threshold.");
            public static GUIContent gapsOverlapsWidth = EditorGUIUtility.TrTextContent("Gaps Overlaps Width", "Set the gaps and overlaps width.");

            public static GUIContent substrateSettngs = EditorGUIUtility.TrTextContent("Substrate Settings", "Set the substrate settings in substrate passes.");
            public static GUIContent substrateTexture = EditorGUIUtility.TrTextContent("Substrate Texture", "Set the substrate texture.");
            public static GUIContent substrateColor = EditorGUIUtility.TrTextContent("Substrate Color", "Set the substrate color.");
            public static GUIContent substrateShading = EditorGUIUtility.TrTextContent("Substrate Shading Intensity", "Set the substrate shading intensity.");
            public static GUIContent substrateLightingDir = EditorGUIUtility.TrTextContent("Substrate Lighting Dir", "Set the substrate lighting direction.");
            public static GUIContent substrateLightingTilt = EditorGUIUtility.TrTextContent("Substrate Lighting Tilt", "Set the substrate lighting tilt.");
            public static GUIContent substrateScale = EditorGUIUtility.TrTextContent("Substrate Scale", "Set the substrate scale.");
            public static GUIContent substrateRoughness = EditorGUIUtility.TrTextContent("Substrate Roughness", "Set the substrate roughness");
            public static GUIContent substrateDistortion = EditorGUIUtility.TrTextContent("Substrate Distortion", "Set the substrate distortion");

            public static GUIContent postprocessSettings = EditorGUIUtility.TrTextContent("PostProcess Settings", "Set the post process params.");
            public static GUIContent postprocessSaturation = EditorGUIUtility.TrTextContent("PostProcess Saturation", "Set the post process saturation.");//in fact this should not be used.
            public static GUIContent postprocessContrast = EditorGUIUtility.TrTextContent("PostProcess Contrast", "Set the post process contrast.");
            public static GUIContent postprocessBrightness = EditorGUIUtility.TrTextContent("PostProcess Brightness", "Set the post process brightness.");

            public static GUIContent shaderVariantLogLevel = EditorGUIUtility.TrTextContent("Shader Variant Log Level", "Controls the level logging in of shader variants information is outputted when a build is performed. Information will appear in the Unity console when the build finishes.");

            // Dropdown menu options
            public static string[] mainLightOptions = { "Disabled", "Per Pixel" };
            public static string[] shadowCascadeOptions = {"No Cascades", "Two Cascades", "Four Cascades"};
            public static string[] opaqueDownsamplingOptions = {"None", "2x (Bilinear)", "4x (Box)", "4x (Bilinear)"};
        }

        SavedBool m_GeneralSettingsFoldout;
        SavedBool m_QualitySettingsFoldout;
        SavedBool m_LightingSettingsFoldout;
        SavedBool m_ShadowSettingsFoldout;
        SavedBool m_AdvancedSettingsFoldout;
        SavedBool m_WatercolorSettingsFoldout;

        SerializedProperty m_RequireDepthTextureProp;
        SerializedProperty m_RequireOpaqueTextureProp;
        SerializedProperty m_OpaqueDownsamplingProp;

        SerializedProperty m_EnableWaterColor;

        SerializedProperty m_HDR;
        SerializedProperty m_MSAA;
        SerializedProperty m_RenderScale;

        SerializedProperty m_MainLightRenderingModeProp;
        SerializedProperty m_MainLightShadowsSupportedProp;
        SerializedProperty m_MainLightShadowmapResolutionProp;

        SerializedProperty m_AdditionalLightsRenderingModeProp;
        SerializedProperty m_AdditionalLightsPerObjectLimitProp;
        SerializedProperty m_AdditionalLightShadowsSupportedProp;
        SerializedProperty m_AdditionalLightShadowmapResolutionProp;

        SerializedProperty m_ShadowDistanceProp;
        SerializedProperty m_ShadowCascadesProp;
        SerializedProperty m_ShadowCascade2SplitProp;
        SerializedProperty m_ShadowCascade4SplitProp;
        SerializedProperty m_ShadowDepthBiasProp;
        SerializedProperty m_ShadowNormalBiasProp;

        SerializedProperty m_SoftShadowsSupportedProp;

        SerializedProperty m_SRPBatcher;
        SerializedProperty m_SupportsDynamicBatching;
        SerializedProperty m_MixedLightingSupportedProp;

        SerializedProperty m_depthRangeProp;
        SerializedProperty m_atmosphereTintProp;
        SerializedProperty m_atmosphereRangeProp;

        SerializedProperty m_pigmentDensityProp;
        SerializedProperty m_edgeDarkenIntensityProp;
        SerializedProperty m_edgeDarkenWidthProp;
        SerializedProperty m_bleedingRadiusProp;
        SerializedProperty m_bleedingThresholdProp;
        SerializedProperty m_drybrushThresholdProp;
        SerializedProperty m_gapsOverlapsWidthProp;

        SerializedProperty m_substrateTextureProp;
        SerializedProperty m_substrateColorProp;
        SerializedProperty m_substrateShadingProp;
        SerializedProperty m_substrateLightingDirProp;
        SerializedProperty m_substrateLightingTiltProp;
        SerializedProperty m_substrateScaleProp;
        SerializedProperty m_substrateRoughnessProp;
        SerializedProperty m_substrateDistortionProp;

        SerializedProperty m_postprocessSaturationProp;
        SerializedProperty m_postprocessContrastProp;
        SerializedProperty m_postprocessBrightnessProp;

        SerializedProperty m_ShaderVariantLogLevel;

        //Water Color Post Process Property

        internal static LightRenderingMode selectedLightRenderingMode;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGeneralSettings();
            DrawQualitySettings();
            DrawLightingSettings();
            DrawShadowSettings();
            DrawAdvancedSettings();
            DrawWatercolorSettings();

            serializedObject.ApplyModifiedProperties();
        }

        void OnEnable()
        {
            m_GeneralSettingsFoldout = new SavedBool($"{target.GetType()}.GeneralSettingsFoldout", false);
            m_QualitySettingsFoldout = new SavedBool($"{target.GetType()}.QualitySettingsFoldout", false);
            m_LightingSettingsFoldout = new SavedBool($"{target.GetType()}.LightingSettingsFoldout", false);
            m_ShadowSettingsFoldout = new SavedBool($"{target.GetType()}.ShadowSettingsFoldout", false);
            m_AdvancedSettingsFoldout = new SavedBool($"{target.GetType()}.AdvancedSettingsFoldout", false);
            m_WatercolorSettingsFoldout = new SavedBool($"{target.GetType()}.WatercolorSettingsFoldout",false);

            //Watercolor settings


            m_RequireDepthTextureProp = serializedObject.FindProperty("m_RequireDepthTexture");
            m_RequireOpaqueTextureProp = serializedObject.FindProperty("m_RequireOpaqueTexture");
            m_OpaqueDownsamplingProp = serializedObject.FindProperty("m_OpaqueDownsampling");
            // this is means to find a serialized property in pipeline asset.
            m_EnableWaterColor = serializedObject.FindProperty("m_EnableWaterColor");

            m_HDR = serializedObject.FindProperty("m_SupportsHDR");
            m_MSAA = serializedObject.FindProperty("m_MSAA");
            m_RenderScale = serializedObject.FindProperty("m_RenderScale");

            m_MainLightRenderingModeProp = serializedObject.FindProperty("m_MainLightRenderingMode");
            m_MainLightShadowsSupportedProp = serializedObject.FindProperty("m_MainLightShadowsSupported");
            m_MainLightShadowmapResolutionProp = serializedObject.FindProperty("m_MainLightShadowmapResolution");

            m_AdditionalLightsRenderingModeProp = serializedObject.FindProperty("m_AdditionalLightsRenderingMode");
            m_AdditionalLightsPerObjectLimitProp = serializedObject.FindProperty("m_AdditionalLightsPerObjectLimit");
            m_AdditionalLightShadowsSupportedProp = serializedObject.FindProperty("m_AdditionalLightShadowsSupported");
            m_AdditionalLightShadowmapResolutionProp = serializedObject.FindProperty("m_AdditionalLightsShadowmapResolution");

            m_ShadowDistanceProp = serializedObject.FindProperty("m_ShadowDistance");
            m_ShadowCascadesProp = serializedObject.FindProperty("m_ShadowCascades");
            m_ShadowCascade2SplitProp = serializedObject.FindProperty("m_Cascade2Split");
            m_ShadowCascade4SplitProp = serializedObject.FindProperty("m_Cascade4Split");
            m_ShadowDepthBiasProp = serializedObject.FindProperty("m_ShadowDepthBias");
            m_ShadowNormalBiasProp = serializedObject.FindProperty("m_ShadowNormalBias");
            m_SoftShadowsSupportedProp = serializedObject.FindProperty("m_SoftShadowsSupported");

            m_SRPBatcher = serializedObject.FindProperty("m_UseSRPBatcher");
            m_SupportsDynamicBatching = serializedObject.FindProperty("m_SupportsDynamicBatching");
            m_MixedLightingSupportedProp = serializedObject.FindProperty("m_MixedLightingSupported");
            //wc
            m_depthRangeProp = serializedObject.FindProperty("m_depthRange");
            m_atmosphereTintProp = serializedObject.FindProperty("m_atmosphereTint");
            m_atmosphereRangeProp = serializedObject.FindProperty("m_atmosphereRange");

            m_pigmentDensityProp = serializedObject.FindProperty("m_pigmentDensity");
            m_edgeDarkenIntensityProp = serializedObject.FindProperty("m_edgeDarkenIntensity");
            m_edgeDarkenWidthProp = serializedObject.FindProperty("m_edgeDarkenWidth");
            m_bleedingThresholdProp = serializedObject.FindProperty("m_bleedingThreshold");
            m_drybrushThresholdProp = serializedObject.FindProperty("m_drybrushThreshold");
            m_gapsOverlapsWidthProp = serializedObject.FindProperty("m_gapsOverlapsWidth");

            m_substrateColorProp = serializedObject.FindProperty("m_substrateColor");
            m_substrateShadingProp = serializedObject.FindProperty("m_substrateShading");
            m_substrateLightingDirProp = serializedObject.FindProperty("m_substrateLightingDir");
            m_substrateLightingTiltProp = serializedObject.FindProperty("m_substrateLightingTilt");
            m_substrateScaleProp = serializedObject.FindProperty("m_substrateScale");
            m_substrateRoughnessProp = serializedObject.FindProperty("m_substrateRoughness");
            m_substrateDistortionProp = serializedObject.FindProperty("m_substrateDistortion");

            m_postprocessSaturationProp = serializedObject.FindProperty("m_postprocessSaturation");
            m_postprocessContrastProp = serializedObject.FindProperty("m_postprocessContrast");
            m_postprocessBrightnessProp = serializedObject.FindProperty("m_postprocessBrightness");
            //wc
            m_ShaderVariantLogLevel = serializedObject.FindProperty("m_ShaderVariantLogLevel");
            selectedLightRenderingMode = (LightRenderingMode)m_AdditionalLightsRenderingModeProp.intValue;
        }

        void DrawGeneralSettings()
        {
            m_GeneralSettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_GeneralSettingsFoldout.value, Styles.generalSettingsText);
            if (m_GeneralSettingsFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_RequireDepthTextureProp, Styles.requireDepthTextureText);
                EditorGUILayout.PropertyField(m_RequireOpaqueTextureProp, Styles.requireOpaqueTextureText);
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(!m_RequireOpaqueTextureProp.boolValue);
                EditorGUILayout.PropertyField(m_OpaqueDownsamplingProp, Styles.opaqueDownsamplingText);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawQualitySettings()
        {
            m_QualitySettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_QualitySettingsFoldout.value, Styles.qualitySettingsText);
            if (m_QualitySettingsFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_HDR, Styles.hdrText);
                EditorGUILayout.PropertyField(m_MSAA, Styles.msaaText);
                EditorGUI.BeginDisabledGroup(XRGraphics.enabled);
                m_RenderScale.floatValue = EditorGUILayout.Slider(Styles.renderScaleText, m_RenderScale.floatValue, LightweightRenderPipeline.minRenderScale, LightweightRenderPipeline.maxRenderScale);
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawLightingSettings()
        {
            m_LightingSettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_LightingSettingsFoldout.value, Styles.lightingSettingsText);
            if (m_LightingSettingsFoldout.value)
            {
                EditorGUI.indentLevel++;

                // Main Light
                bool disableGroup = false;
                EditorGUI.BeginDisabledGroup(disableGroup);
                CoreEditorUtils.DrawPopup(Styles.mainLightRenderingModeText, m_MainLightRenderingModeProp, Styles.mainLightOptions);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel++;
                disableGroup |= !m_MainLightRenderingModeProp.boolValue;

                EditorGUI.BeginDisabledGroup(disableGroup);
                EditorGUILayout.PropertyField(m_MainLightShadowsSupportedProp, Styles.supportsMainLightShadowsText);
                EditorGUI.EndDisabledGroup();

                disableGroup |= !m_MainLightShadowsSupportedProp.boolValue;
                EditorGUI.BeginDisabledGroup(disableGroup);
                EditorGUILayout.PropertyField(m_MainLightShadowmapResolutionProp, Styles.mainLightShadowmapResolutionText);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                // Additional light
                selectedLightRenderingMode = (LightRenderingMode)EditorGUILayout.EnumPopup(Styles.addditionalLightsRenderingModeText, selectedLightRenderingMode);
                m_AdditionalLightsRenderingModeProp.intValue = (int)selectedLightRenderingMode;
                EditorGUI.indentLevel++;

                disableGroup = m_AdditionalLightsRenderingModeProp.intValue == (int)LightRenderingMode.Disabled;
                EditorGUI.BeginDisabledGroup(disableGroup);
                m_AdditionalLightsPerObjectLimitProp.intValue = EditorGUILayout.IntSlider(Styles.perObjectLimit, m_AdditionalLightsPerObjectLimitProp.intValue, 0, LightweightRenderPipeline.maxPerObjectLightCount);
                EditorGUI.EndDisabledGroup();

                disableGroup |= (m_AdditionalLightsPerObjectLimitProp.intValue == 0 || m_AdditionalLightsRenderingModeProp.intValue != (int)LightRenderingMode.PerPixel);
                EditorGUI.BeginDisabledGroup(disableGroup);
                EditorGUILayout.PropertyField(m_AdditionalLightShadowsSupportedProp, Styles.supportsAdditionalShadowsText);
                EditorGUI.EndDisabledGroup();

                disableGroup |= !m_AdditionalLightShadowsSupportedProp.boolValue;
                EditorGUI.BeginDisabledGroup(disableGroup);
                EditorGUILayout.PropertyField(m_AdditionalLightShadowmapResolutionProp, Styles.additionalLightsShadowmapResolution);
                EditorGUI.EndDisabledGroup();

                EditorGUI.indentLevel--;
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawShadowSettings()
        {
            m_ShadowSettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_ShadowSettingsFoldout.value, Styles.shadowSettingsText);
            if (m_ShadowSettingsFoldout.value)
            {
                EditorGUI.indentLevel++;
                m_ShadowDistanceProp.floatValue = Mathf.Max(0.0f, EditorGUILayout.FloatField(Styles.shadowDistanceText, m_ShadowDistanceProp.floatValue));
                CoreEditorUtils.DrawPopup(Styles.shadowCascadesText, m_ShadowCascadesProp, Styles.shadowCascadeOptions);

                ShadowCascadesOption cascades = (ShadowCascadesOption)m_ShadowCascadesProp.intValue;
                if (cascades == ShadowCascadesOption.FourCascades)
                    LightweightRenderPipelineEditorUtils.DrawCascadeSplitGUI<Vector3>(ref m_ShadowCascade4SplitProp);
                else if (cascades == ShadowCascadesOption.TwoCascades)
                    LightweightRenderPipelineEditorUtils.DrawCascadeSplitGUI<float>(ref m_ShadowCascade2SplitProp);

                m_ShadowDepthBiasProp.floatValue = EditorGUILayout.Slider(Styles.shadowDepthBias, m_ShadowDepthBiasProp.floatValue, 0.0f, LightweightRenderPipeline.maxShadowBias);
                m_ShadowNormalBiasProp.floatValue = EditorGUILayout.Slider(Styles.shadowNormalBias, m_ShadowNormalBiasProp.floatValue, 0.0f, LightweightRenderPipeline.maxShadowBias);
                EditorGUILayout.PropertyField(m_SoftShadowsSupportedProp, Styles.supportsSoftShadows);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawAdvancedSettings()
        {
            m_AdvancedSettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_AdvancedSettingsFoldout.value, Styles.advancedSettingsText);
            if (m_AdvancedSettingsFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_SRPBatcher, Styles.srpBatcher);
                EditorGUILayout.PropertyField(m_SupportsDynamicBatching, Styles.dynamicBatching);
                EditorGUILayout.PropertyField(m_MixedLightingSupportedProp, Styles.mixedLightingSupportLabel);
                EditorGUILayout.PropertyField(m_ShaderVariantLogLevel, Styles.shaderVariantLogLevel);
                //EditorGUILayout.PropertyField(m_EnableWaterColor, Styles.WaterColorEnableText);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        void DrawWatercolorSettings()
        {
            m_WatercolorSettingsFoldout.value = EditorGUILayout.BeginFoldoutHeaderGroup(m_WatercolorSettingsFoldout.value, Styles.watercolorSettingsText);
            if (m_WatercolorSettingsFoldout.value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(m_depthRangeProp, Styles.depthRange);
                EditorGUILayout.PropertyField(m_atmosphereTintProp, Styles.atmosphereTint);
                EditorGUILayout.PropertyField(m_atmosphereRangeProp, Styles.atmosphereRange);
                EditorGUILayout.PropertyField(m_pigmentDensityProp, Styles.pigmentDensity);
                EditorGUILayout.PropertyField(m_edgeDarkenIntensityProp, Styles.edgeDarkenIntensity);
                EditorGUILayout.PropertyField(m_edgeDarkenWidthProp, Styles.edgeDarkenWidth);
                EditorGUILayout.PropertyField(m_bleedingThresholdProp, Styles.bleedingThreshold);
                EditorGUILayout.PropertyField(m_drybrushThresholdProp, Styles.drybrushThreshold);
                EditorGUILayout.PropertyField(m_gapsOverlapsWidthProp, Styles.gapsOverlapsWidth);
                EditorGUILayout.PropertyField(m_substrateColorProp, Styles.substrateColor);
                EditorGUILayout.PropertyField(m_substrateShadingProp, Styles.substrateShading);
                EditorGUILayout.PropertyField(m_substrateLightingDirProp, Styles.substrateLightingDir);
                EditorGUILayout.PropertyField(m_substrateLightingTiltProp, Styles.substrateLightingTilt);
                EditorGUILayout.PropertyField(m_substrateScaleProp, Styles.substrateScale);
                EditorGUILayout.PropertyField(m_substrateRoughnessProp, Styles.substrateRoughness);
                EditorGUILayout.PropertyField(m_substrateDistortionProp, Styles.substrateDistortion);
                EditorGUILayout.PropertyField(m_postprocessSaturationProp, Styles.postprocessSaturation);
                EditorGUILayout.PropertyField(m_postprocessContrastProp, Styles.postprocessContrast);
                EditorGUILayout.PropertyField(m_postprocessBrightnessProp, Styles.postprocessBrightness);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
