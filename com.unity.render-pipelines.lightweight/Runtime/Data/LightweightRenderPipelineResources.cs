using UnityEngine.Serialization;

namespace UnityEngine.Rendering.LWRP
{
    public class LightweightRenderPipelineResources : ScriptableObject
    {
        [FormerlySerializedAs("BlitShader"), SerializeField] Shader m_BlitShader = null;
        [FormerlySerializedAs("CopyDepthShader"), SerializeField] Shader m_CopyDepthShader = null;
        [FormerlySerializedAs("ScreenSpaceShadowShader"), SerializeField] Shader m_ScreenSpaceShadowShader = null;
        [FormerlySerializedAs("SamplingShader"), SerializeField] Shader m_SamplingShader = null;
        [FormerlySerializedAs("QuadAdjustLoad"), SerializeField] Shader m_AdjustLoad = null;
        [FormerlySerializedAs("QuadEdgeDetection"), SerializeField] Shader m_EdgeDetection = null;
        [FormerlySerializedAs("QuadPigmentManipulation"), SerializeField] Shader m_PigmentDensity = null;
        [FormerlySerializedAs("QuadSeparable"), SerializeField] Shader m_Separable = null;
        [FormerlySerializedAs("QuadBlend"), SerializeField] Shader m_Blend = null;
        [FormerlySerializedAs("QuadEdgeManipulation"), SerializeField] Shader m_EdgeManipulation = null;
        [FormerlySerializedAs("QuadGapsOverlaps"), SerializeField] Shader m_GapsOverlaps = null;
        [FormerlySerializedAs("QuadPigmentApplication"), SerializeField] Shader m_PigmentApplication = null;
        [FormerlySerializedAs("QuadSubstrate"), SerializeField] Shader m_Substrate = null;
        [FormerlySerializedAs("CreateLinearDepthShader"), SerializeField] Shader m_CreateLinearDepthShader = null;
        [FormerlySerializedAs("Substrate Back"), SerializeField] Texture m_SubstrateBack = null;

        public Texture SubstrateBack
        {
            get { return m_SubstrateBack; }
        }
        public Shader AdjustLoad
        {
            get { return m_AdjustLoad; }
        }
        public Shader EdgeDetection
        {
            get { return m_EdgeDetection; }
        }
        public Shader CreateLinearDepth
        {
            get { return m_CreateLinearDepthShader; }
        }

        public Shader PigmentDensity
        {
            get { return m_PigmentDensity; }
        }

        public Shader Separable
        {
            get { return m_Separable; }
        }

        public Shader Blend
        {
            get { return m_Blend; }
        }

        public Shader EdgeManipulation
        {
            get { return m_EdgeManipulation; }
        }

        public Shader GapsOverlaps
        {
            get { return m_GapsOverlaps; }
        }

        public Shader PigmentApplication
        {
            get { return m_PigmentApplication; }
        }

        public Shader Substrate
        {
            get { return m_Substrate; }
        }

        public Shader blitShader
        {
            get { return m_BlitShader; }
        }

        public Shader copyDepthShader
        {
            get { return m_CopyDepthShader; }
        }

        public Shader screenSpaceShadowShader
        {
            get { return m_ScreenSpaceShadowShader; }
        }

        public Shader samplingShader
        {
            get { return m_SamplingShader; }
        }
    }
}
