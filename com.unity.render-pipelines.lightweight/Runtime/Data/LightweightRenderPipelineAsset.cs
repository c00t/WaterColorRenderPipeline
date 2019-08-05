#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif

using UnityEngine.Experimental.Rendering.LWRP;

namespace UnityEngine.Rendering.LWRP
{
    public enum ShadowCascadesOption
    {
        NoCascades,
        TwoCascades,
        FourCascades,
    }

    public enum ShadowQuality
    {
        Disabled,
        HardShadows,
        SoftShadows,
    }

    public enum ShadowResolution
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096
    }

    public enum MsaaQuality
    {
        Disabled = 1,
        _2x = 2,
        _4x = 4,
        _8x = 8
    }

    public enum Downsampling
    {
        None,
        _2xBilinear,
        _4xBox,
        _4xBilinear
    }

    internal enum DefaultMaterialType
    {
        Standard,
        Particle,
        Terrain,
        UnityBuiltinDefault
    }

    public enum LightRenderingMode
    {
        Disabled = 0,
        PerVertex = 2,
        PerPixel = 1,
    }

    public enum ShaderVariantLogLevel
    {
        Disabled,
        OnlyLightweightRPShaders,
        AllShaders,
    }

    public class LightweightRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver
    {
        Shader m_DefaultShader;
        internal IRendererSetup m_RendererSetup;

        // Default values set when a new LightweightRenderPipeline asset is created
        [SerializeField] int k_AssetVersion = 4;

        // General settings
        [SerializeField] bool m_RequireDepthTexture = false;
        [SerializeField] bool m_RequireOpaqueTexture = false;
        [SerializeField] Downsampling m_OpaqueDownsampling = Downsampling._2xBilinear;



        // Quality settings
        [SerializeField] bool m_SupportsHDR = false;
        [SerializeField] MsaaQuality m_MSAA = MsaaQuality.Disabled;
        [SerializeField] float m_RenderScale = 1.0f;
        // TODO: Shader Quality Tiers

        // Main directional light Settings
        [SerializeField] LightRenderingMode m_MainLightRenderingMode = LightRenderingMode.PerPixel;
        [SerializeField] bool m_MainLightShadowsSupported = true;
        [SerializeField] ShadowResolution m_MainLightShadowmapResolution = ShadowResolution._2048;

        // Additional lights settings
        [SerializeField] LightRenderingMode m_AdditionalLightsRenderingMode = LightRenderingMode.PerPixel;
        [SerializeField] int m_AdditionalLightsPerObjectLimit = 4;
        [SerializeField] bool m_AdditionalLightShadowsSupported = false;
        [SerializeField] ShadowResolution m_AdditionalLightsShadowmapResolution = ShadowResolution._512;

        // Shadows Settings
        [SerializeField] float m_ShadowDistance = 50.0f;
        [SerializeField] ShadowCascadesOption m_ShadowCascades = ShadowCascadesOption.NoCascades;
        [SerializeField] float m_Cascade2Split = 0.25f;
        [SerializeField] Vector3 m_Cascade4Split = new Vector3(0.067f, 0.2f, 0.467f);
        [SerializeField] float m_ShadowDepthBias = 1.0f;
        [SerializeField] float m_ShadowNormalBias = 1.0f;
        [SerializeField] bool m_SoftShadowsSupported = false;

        // Advanced settings
        [SerializeField] bool m_UseSRPBatcher = true;
        [SerializeField] bool m_SupportsDynamicBatching = false;
        [SerializeField] bool m_MixedLightingSupported = true;

        // Water Color settings properties
        [SerializeField] bool m_EnableWaterColor = true;
        [SerializeField] Vector2 m_depthRange = new Vector2(8.0f,50.0f);
        [SerializeField] Color m_atmosphereTint = Color.white;
        [SerializeField] Vector2 m_atmosphereRange = new Vector2(25.0f, 300.0f);
        [SerializeField] float m_pigmentDensity = 5.0f;
        [SerializeField] float m_edgeDarkenIntensity = 1.0f;
        [SerializeField] int m_edgeDarkenWidth = 3;
        [SerializeField] float m_bleedingThreshold = 0.0002f;
        [SerializeField] float m_drybrushThreshold = 15.0f;
        [SerializeField] int m_gapsOverlapsWidth = 3;
        [SerializeField] Color m_substrateColor = Color.white;
        [SerializeField] float m_substrateShading = 0.5f;
        [SerializeField] float m_substrateLightingDir = 180.0f;
        [SerializeField] float m_substrateLightingTilt = 45.0f;
        [SerializeField] float m_substrateScale = 1.0f;
        [SerializeField] float m_substrateRoughness = 1.0f;
        [SerializeField] float m_substrateDistortion = 1.0f;
        [SerializeField] float m_postprocessSaturation = 1.0f;
        [SerializeField] float m_postprocessContrast = 1.0f;
        [SerializeField] float m_postprocessBrightness = 1.0f;

        // Deprecated settings
        [SerializeField] ShadowQuality m_ShadowType = ShadowQuality.HardShadows;
        [SerializeField] bool m_LocalShadowsSupported = false;
        [SerializeField] ShadowResolution m_LocalShadowsAtlasResolution = ShadowResolution._256;
        [SerializeField] int m_MaxPixelLights = 0;
        [SerializeField] ShadowResolution m_ShadowAtlasResolution = ShadowResolution._256;

        [SerializeField] LightweightRenderPipelineResources m_ResourcesAsset = null;
        [SerializeField] ShaderVariantLogLevel m_ShaderVariantLogLevel = ShaderVariantLogLevel.Disabled;

#if UNITY_EDITOR
        [NonSerialized]
        LightweightRenderPipelineEditorResources m_EditorResourcesAsset;

        static readonly string s_SearchPathProject = "Assets";
        static readonly string s_SearchPathPackage = "Packages/com.unity.render-pipelines.lightweight";

        public static LightweightRenderPipelineAsset Create()
        {
            var instance = CreateInstance<LightweightRenderPipelineAsset>();
            instance.m_EditorResourcesAsset = LoadResourceFile<LightweightRenderPipelineEditorResources>();
            instance.m_ResourcesAsset = LoadResourceFile<LightweightRenderPipelineResources>();
            return instance;
        }
 
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812")]
        internal class CreateLightweightPipelineAsset : EndNameEditAction
        {
            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                AssetDatabase.CreateAsset(Create(), pathName);
            }
        }

        [MenuItem("Assets/Create/Rendering/WaterColor Render Pipeline Asset", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateLightweightPipeline()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateLightweightPipelineAsset>(),
                "WaterColorRenderPipelineAsset.asset", null, null);
        }
/// these two seems no useful
        //[MenuItem("Assets/Create/Rendering/Lightweight Pipeline Resources", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateLightweightPipelineResources()
        {
            var instance = CreateInstance<LightweightRenderPipelineResources>();
            AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", typeof(LightweightRenderPipelineResources).Name));
        }

        //[MenuItem("Assets/Create/Rendering/Lightweight Pipeline Editor Resources", priority = CoreUtils.assetCreateMenuPriority1)]
        static void CreateLightweightPipelineEditorResources()
        {
            var instance = CreateInstance<LightweightRenderPipelineEditorResources>();
            AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", typeof(LightweightRenderPipelineEditorResources).Name));
        }

        static T LoadResourceFile<T>() where T : ScriptableObject
        {
            T resourceAsset = null;
            var guids = AssetDatabase.FindAssets(typeof(T).Name + " t:scriptableobject", new[] {s_SearchPathProject});
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                resourceAsset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (resourceAsset != null)
                    break;
            }

            // There's currently an issue that prevents FindAssets from find resources withing the package folder.
            if (resourceAsset == null)
            {
                string path = s_SearchPathPackage + "/Runtime/Data/" + typeof(T).Name + ".asset";
                resourceAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            }
            return resourceAsset;
        }

        LightweightRenderPipelineEditorResources editorResources
        {
            get
            {
                if (m_EditorResourcesAsset == null)
                    m_EditorResourcesAsset = LoadResourceFile<LightweightRenderPipelineEditorResources>();

                return m_EditorResourcesAsset;
            }
        }
#endif
        LightweightRenderPipelineResources resources
        {
            get
            {
#if UNITY_EDITOR
                if (m_ResourcesAsset == null)
                    m_ResourcesAsset = LoadResourceFile<LightweightRenderPipelineResources>();
#endif
                return m_ResourcesAsset;
            }
        }

        protected override RenderPipeline CreatePipeline()
        {
            return new LightweightRenderPipeline(this);
        }

        Material GetMaterial(DefaultMaterialType materialType)
        {
#if UNITY_EDITOR
            if (editorResources == null)
                return null;

            switch (materialType)
            {
                case DefaultMaterialType.Standard:
                    return editorResources.litMaterial;

                case DefaultMaterialType.Particle:
                    return editorResources.particleLitMaterial;

                case DefaultMaterialType.Terrain:
                    return editorResources.terrainLitMaterial;

                // Unity Builtin Default
                default:
                    return null;
            }
#else
            return null;
#endif
        }

        public IRendererSetup rendererSetup
        {
            get { return m_RendererSetup; }
        }

        public bool supportsCameraDepthTexture
        {
            get { return m_RequireDepthTexture; }
            set { m_RequireDepthTexture = value; }
        }

        public bool supportsCameraOpaqueTexture
        {
            get { return m_RequireOpaqueTexture; }
            set { m_RequireOpaqueTexture = value; }
        }

        public Downsampling opaqueDownsampling
        {
            get { return m_OpaqueDownsampling; }
            set { m_OpaqueDownsampling = value; }
        }

        public bool enableWaterColor
        {
            get { return m_EnableWaterColor; }
            set { m_EnableWaterColor = value; }
        }

        public bool supportsHDR
        {
            get { return m_SupportsHDR; }
            set { m_SupportsHDR = value; }
        }

        public int msaaSampleCount
        {
            get { return (int)m_MSAA; }
            set { m_MSAA = (MsaaQuality)value; }
        }

        public float renderScale
        {
            get { return m_RenderScale; }
            set { m_RenderScale = ValidateRenderScale(value); }
        }

        public LightRenderingMode mainLightRenderingMode
        {
            get { return m_MainLightRenderingMode; }
        }

        public bool supportsMainLightShadows
        {
            get { return m_MainLightShadowsSupported; }
        }

        public int mainLightShadowmapResolution
        {
            get { return (int)m_MainLightShadowmapResolution; }
        }

        public LightRenderingMode additionalLightsRenderingMode
        {
            get { return m_AdditionalLightsRenderingMode; }
        }

        public int maxAdditionalLightsCount
        {
            get { return m_AdditionalLightsPerObjectLimit; }
            set { m_AdditionalLightsPerObjectLimit = ValidatePerObjectLights(value); }
        }

        public bool supportsAdditionalLightShadows
        {
            get { return m_AdditionalLightShadowsSupported; }
        }

        public int additionalLightsShadowmapResolution
        {
            get { return (int)m_AdditionalLightsShadowmapResolution; }
        }

        public float shadowDistance
        {
            get { return m_ShadowDistance; }
            set { m_ShadowDistance = Mathf.Max(0.0f, value); }
        }

        public ShadowCascadesOption shadowCascadeOption
        {
            get { return m_ShadowCascades; }
            set { m_ShadowCascades = value; }
        }

        public float cascade2Split
        {
            get { return m_Cascade2Split; }
        }

        public Vector3 cascade4Split
        {
            get { return m_Cascade4Split; }
        }

        public float shadowDepthBias
        {
            get { return m_ShadowDepthBias; }
            set { m_ShadowDepthBias = ValidateShadowBias(value); }
        }

        public float shadowNormalBias
        {
            get { return m_ShadowNormalBias; }
            set { m_ShadowNormalBias = ValidateShadowBias(value); }
        }

        public bool supportsSoftShadows
        {
            get { return m_SoftShadowsSupported; }
        }

        public bool supportsDynamicBatching
        {
            get { return m_SupportsDynamicBatching; }
            set { m_SupportsDynamicBatching = value; }
        }

        public bool supportsMixedLighting
        {
            get { return m_MixedLightingSupported; }
        }

        public ShaderVariantLogLevel shaderVariantLogLevel
        {
            get { return m_ShaderVariantLogLevel; }
            set { m_ShaderVariantLogLevel = value; }
        }
        
        public bool useSRPBatcher
        {
            get { return m_UseSRPBatcher; }
            set { m_UseSRPBatcher = value; }
        }

        public override Material defaultMaterial
        {
            get { return GetMaterial(DefaultMaterialType.Standard); }
        }

        public override Material defaultParticleMaterial
        {
            get { return GetMaterial(DefaultMaterialType.Particle); }
        }

        public override Material defaultLineMaterial
        {
            get { return GetMaterial(DefaultMaterialType.Particle); }
        }

        public override Material defaultTerrainMaterial
        {
            get { return GetMaterial(DefaultMaterialType.Terrain); }
        }

        public override Material defaultUIMaterial
        {
            get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material defaultUIOverdrawMaterial
        {
            get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material defaultUIETC1SupportedMaterial
        {
            get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Material default2DMaterial
        {
            get { return GetMaterial(DefaultMaterialType.UnityBuiltinDefault); }
        }

        public override Shader defaultShader
        {
            get
            {
                if (m_DefaultShader == null)
                    m_DefaultShader = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.PhysicallyBased));
                return m_DefaultShader;
            }
        }

#if UNITY_EDITOR
        public override Shader autodeskInteractiveShader
        {
            get { return editorResources.autodeskInteractiveShader; }
        }

        public override Shader autodeskInteractiveTransparentShader
        {
            get { return editorResources.autodeskInteractiveTransparentShader; }
        }

        public override Shader autodeskInteractiveMaskedShader
        {
            get { return editorResources.autodeskInteractiveMaskedShader; }
        }
#endif
        //[SerializeField] bool m_EnableWaterColor = true;
        //[SerializeField] Vector2 m_depthRange = new Vector2(8.0f, 50.0f);=
        //[SerializeField] Color m_atmosphereTint = Color.white;=
        //[SerializeField] Vector2 m_atmosphereRange = new Vector2(25.0f, 300.0f);=
        //[SerializeField] float m_pigmentDensity = 5.0f;=
        //[SerializeField] float m_edgeDarkenIntensity = 1.0f;=
        //[SerializeField] int m_edgeDarkenWidth = 3;=
        //[SerializeField] int m_bleedingRadius = 10;//not used
        //[SerializeField] float m_bleedingThreshold = 0.0002f;=
        //[SerializeField] float m_drybrushThreshold = 15.0f;=
        //[SerializeField] int m_gapsOverlapsWidth = 3;=
        //[SerializeField] Texture m_substrateTexture = null;
        //[SerializeField] Color m_substrateColor = Color.white;=
        //[SerializeField] float m_substrateShading = 0.5f;=
        //[SerializeField] float m_substrateLightingDir = 180.0f;=
        //[SerializeField] float m_substrateLightingTilt = 45.0f;=
        //[SerializeField] float m_substrateScale = 1.0f;=
        //[SerializeField] float m_substrateRoughness = 1.0f;=
        //[SerializeField] float m_substrateDistortion = 1.0f;=
        //[SerializeField] float m_postprocessSaturation = 1.0f;=
        //[SerializeField] float m_postprocessContrast = 1.0f;=
        //[SerializeField] float m_postprocessBrightness = 1.0f;=
        public Vector2 DepthRange
        {
            get { return m_depthRange; }
        }
        public Color AtmosphereTint
        {
            get { return m_atmosphereTint; }
        }
        public Vector2 AtmosphereRange
        {
            get { return m_atmosphereRange; }
        }
        public float PigmentDensityParam
        {
            get { return m_pigmentDensity; }
        }
        public float EdgeDarkenIntensity
        {
            get { return m_edgeDarkenIntensity; }
        }
        public int EdgeDarkenWidth
        {
            get { return m_edgeDarkenWidth; }
        }

        public float BleedingThreshold
        {
            get { return m_bleedingThreshold; }
        }
        public float DrybrushThreshold
        {
            get { return m_drybrushThreshold; }
        }
        public int GapsOverlapsWidth
        {
            get { return m_gapsOverlapsWidth; }
        }

        public Color SubstrateColor
        {
            get { return m_substrateColor; }
        }
        public float SubstrateShading
        {
            get { return m_substrateShading; }
        }
        public float SubstrateLightingDir
        {
            get { return m_substrateLightingDir; }
        }
        public float SubstrateLightingTilt
        {
            get { return m_substrateLightingTilt; }
        }
        public float SubstrateScale
        {
            get { return m_substrateScale; }
        }
        public float SubstrateRoughness
        {
            get { return m_substrateRoughness; }
        }
        public float SubstrateDistortion
        {
            get { return m_substrateDistortion; }
        }
        public float PostprocessSaturation
        {
            get { return m_postprocessSaturation; }
        }
        public float PostprocessContrast
        {
            get { return m_postprocessContrast; }
        }
        public float PostprocessBrightness
        {
            get { return m_postprocessBrightness; }
        }


        public Shader AdjustLoad
        {
            get { return resources != null ? resources.AdjustLoad : null; }
        }

        public Shader CreateLinearDepth
        {
            get { return resources != null ? resources.CreateLinearDepth : null; }
        }

        public Shader EdgeDetection
        {
            get { return resources != null ? resources.EdgeDetection : null; }
        }

        public Shader PigmentDensity
        {
            get { return resources != null ? resources.PigmentDensity : null; }
        }

        public Shader Separable
        {
            get { return resources != null ? resources.Separable : null; }
        }

        public Shader Blend
        {
            get { return resources != null ? resources.Blend : null; }
        }

        public Shader EdgeManipulation
        {
            get { return resources != null ? resources.EdgeManipulation : null; }
        }

        public Shader GapsOverlaps
        {
            get { return resources != null ? resources.GapsOverlaps : null; }
        }

        public Shader PigmentApplication
        {
            get { return resources != null ? resources.PigmentApplication : null; }
        }

        public Shader Substrate
        {
            get { return resources != null ? resources.Substrate : null; }
        }

        public Shader blitShader
        {
            get { return resources != null ? resources.blitShader : null; }
        }

        public Texture SubstrateBack
        {
            get { return resources != null ? resources.SubstrateBack : null; }
        }

        public Shader copyDepthShader
        {
            get { return resources != null ? resources.copyDepthShader : null; }
        }

        public Shader screenSpaceShadowShader
        {
            get { return resources != null ? resources.screenSpaceShadowShader : null; }
        }

        public Shader samplingShader
        {
            get { return resources != null ? resources.samplingShader : null; }
        }


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            if (k_AssetVersion < 3)
            {
                k_AssetVersion = 3;
                m_SoftShadowsSupported = (m_ShadowType == ShadowQuality.SoftShadows);
            }

            if (k_AssetVersion < 4)
            {
                k_AssetVersion = 4;
                m_AdditionalLightShadowsSupported = m_LocalShadowsSupported;
                m_AdditionalLightsShadowmapResolution = m_LocalShadowsAtlasResolution;
                m_AdditionalLightsPerObjectLimit = m_MaxPixelLights;
                m_MainLightShadowmapResolution = m_ShadowAtlasResolution;
            }
        }

        float ValidateShadowBias(float value)
        {
            return Mathf.Max(0.0f, Mathf.Min(value, LightweightRenderPipeline.maxShadowBias));
        }

        int ValidatePerObjectLights(int value)
        {
            return System.Math.Max(0, System.Math.Min(value, LightweightRenderPipeline.maxPerObjectLightCount));
        }

        float ValidateRenderScale(float value)
        {
            return Mathf.Max(LightweightRenderPipeline.minRenderScale, Mathf.Min(value, LightweightRenderPipeline.maxRenderScale));
        }
    }
}
