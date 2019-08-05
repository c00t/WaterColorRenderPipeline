using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.LWRP
{
    public enum MixedLightingSetup
    {
        None,
        ShadowMask,
        Subtractive,
    };

    public struct RenderingData
    {
        public CullingResults cullResults;
        public CameraData cameraData;
        public LightData lightData;
        public ShadowData shadowData;
        public bool supportsDynamicBatching;
        public bool killAlphaInFinalBlit;
        public bool enableWC;
        public WatercolorData watercolorData;
    }

    public struct WatercolorData
    {
        public Vector2 depthRange;
        public Color atmosphereTint;
        public Vector2 atmosphereRange;
        public float pigmentDensity;
        public float edgeDarkenIntensity;
        public int edgeDarkenWidth;
        public float bleedingThreshold;
        public float drybrushThreshold;
        public int gapsOverlapsWidth;
        public Color substrateColor;
        public float substrateShading;
        public float substrateLightingDir;
        public float substrateLightingTilt;
        public float substrateScale;
        public float substrateRoughness;
        public float substrateDistortion;
        public float postprocessSaturation;
        public float postprocessContrast;
        public float postprocessBrightness;
    }

    public struct LightData
    {
        public int mainLightIndex;
        public int additionalLightsCount;
        public int maxPerObjectAdditionalLightsCount;
        public NativeArray<VisibleLight> visibleLights;
        public bool shadeAdditionalLightsPerVertex;
        public bool supportsMixedLighting;
    }

    public struct CameraData
    {
        public Camera camera;
        public float renderScale;
        public int msaaSamples;
        public bool isSceneViewCamera;
        public bool isDefaultViewport;
        public bool isHdrEnabled;
        public bool requiresDepthTexture;
        public bool requiresOpaqueTexture;
        public Downsampling opaqueTextureDownsampling;

        public SortingCriteria defaultOpaqueSortFlags;

        public bool isStereoEnabled;

        public float maxShadowDistance;
        public bool postProcessEnabled;
        public PostProcessLayer postProcessLayer;
        public IEnumerator<Action<RenderTargetIdentifier, CommandBuffer> > captureActions;
    }

    public struct ShadowData
    {
        public bool supportsMainLightShadows;
        public bool requiresScreenSpaceShadowResolve;
        public int mainLightShadowmapWidth;
        public int mainLightShadowmapHeight;
        public int mainLightShadowCascadesCount;
        public Vector3 mainLightShadowCascadesSplit;
        public bool supportsAdditionalLightShadows;
        public int additionalLightsShadowmapWidth;
        public int additionalLightsShadowmapHeight;
        public bool supportsSoftShadows;
        public int shadowmapDepthBufferBits;
        public List<Vector4> bias;
    }

    public static class ShaderKeywordStrings
    {
        public static readonly string MainLightShadows = "_MAIN_LIGHT_SHADOWS";
        public static readonly string MainLightShadowCascades = "_MAIN_LIGHT_SHADOWS_CASCADE";
        public static readonly string AdditionalLightsVertex = "_ADDITIONAL_LIGHTS_VERTEX";
        public static readonly string AdditionalLightsPixel = "_ADDITIONAL_LIGHTS";
        public static readonly string AdditionalLightShadows = "_ADDITIONAL_LIGHT_SHADOWS";
        public static readonly string SoftShadows = "_SHADOWS_SOFT";
        public static readonly string MixedLightingSubtractive = "_MIXED_LIGHTING_SUBTRACTIVE";

        public static readonly string DepthNoMsaa = "_DEPTH_NO_MSAA";
        public static readonly string DepthMsaa2 = "_DEPTH_MSAA_2";
        public static readonly string DepthMsaa4 = "_DEPTH_MSAA_4";

        //// Water Color Engine usage
        ///Procedural Process
        //Pigment
        //public static readonly string PigVarProce = "_PIG_VAR_PROCE";
        //public static readonly string PigAppProce = "_PIG_APP_PROCE";
        //public static readonly string PigDenProce = "_PIG_DEN_PROCE";
        ////Substrate
        //public static readonly string SubDisProce = "_SUB_DIS_PROCE";
        //public static readonly string SubUincProce = "_SUB_UINC_PROCE";
        //public static readonly string SubVincProce = "_SUB_VINC_PROCE";
        ////Edge
        //public static readonly string EdgeEdgeProce = "_EDGE_EDGE_PROCE";
        //public static readonly string _EDGE_TRANS_PROCE = "";
        /// I think they are not the pipeline specific value. they are just material specific value.

        public static readonly string LinearToSRGBConversion = "_LINEAR_TO_SRGB_CONVERSION";
        public static readonly string KillAlpha = "_KILL_ALPHA";
    }

    public sealed partial class LightweightRenderPipeline
    {
        static List<Vector4> m_ShadowBiasData = new List<Vector4>();

        public static bool IsStereoEnabled(Camera camera)
        {
            if (camera == null)
                throw new ArgumentNullException("camera");

            bool isGameCamera = camera.cameraType == CameraType.Game || camera.cameraType == CameraType.VR;
            return XRGraphics.enabled && isGameCamera && (camera.stereoTargetEye == StereoTargetEyeMask.Both);
        }

        void SortCameras(Camera[] cameras)
        {
            Array.Sort(cameras, (lhs, rhs) => (int)(lhs.depth - rhs.depth));
        }
    }
}
