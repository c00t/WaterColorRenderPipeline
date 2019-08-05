using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
/*
主要目的是设置好pass和RT的handle
 */
namespace UnityEngine.Experimental.Rendering.LWRP
{
    internal class ForwardRendererSetup : IRendererSetup
    {
        private DepthOnlyPass m_DepthOnlyPass;
        private MainLightShadowCasterPass m_MainLightShadowCasterPass;
        private AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
        private SetupForwardRenderingPass m_SetupForwardRenderingPass;
        private ScreenSpaceShadowResolvePass m_ScreenSpaceShadowResolvePass;
        private CreateLightweightRenderTexturesPass m_CreateLightweightRenderTexturesPass;
        private CreateWaterColorRenderTexturesPass m_CreateWaterColorRenderTexturesPass;
        private BeginXRRenderingPass m_BeginXrRenderingPass;
        private SetupLightweightConstanstPass m_SetupLightweightConstants;
        private RenderOpaqueForwardPass m_RenderOpaqueForwardPass;
        private PostProcessPass m_OpaquePostProcessPass;
        private DrawSkyboxPass m_DrawSkyboxPass;
        private CopyDepthPass m_CopyDepthPass;
        private CreateLinearDepthPass m_CreateLinearDepthPass;
        private CopyColorPass m_CopyColorPass;
        private RenderWaterColorProxyForwardPass m_RenderWaterColorProxyForwardPass;
        private RenderTransparentForwardPass m_RenderTransparentForwardPass;
        //customed for watercolor
        private QuadAdjustLoadPass m_QuadAdjustLoadPass;
        private QuadEdgeDetectionPass m_QuadEdgeDetectionPass;
        private QuadPigmentManipulationPass m_QuadPigmentManipulationPass;
        private QuadSeparableHPass m_QuadSeparableHPass;
        private QuadSeparableVPass m_QuadSeparableVPass;
        private QuadBlendPass m_QuadBlendPass;
        private QuadEdgeManipulationPass m_QuadEdgeManipulationPass;
        private QuadGapsOverlapsPass m_QuadGapsOverlapsPass;
        private QuadPigmentApplicationPass m_QuadPigmentApplicationPass;
        private QuadSubstratePass m_QuadSubstratePass;
        private QuadSubstrateLightingPass m_QuadSubstrateLightingPass;

        private PostProcessPass m_PostProcessPass;
        private CreateColorRenderTexturesPass m_createColorPass;
        private FinalBlitPass m_FinalBlitPass;
        private CapturePass m_CapturePass;
        private EndXRRenderingPass m_EndXrRenderingPass;

        
#if UNITY_EDITOR
        private GizmoRenderingPass m_LitGizmoRenderingPass;
        private GizmoRenderingPass m_UnlitGizmoRenderingPass;
        private SceneViewDepthCopyPass m_SceneViewDepthCopyPass;
#endif


        private RenderTargetHandle m_ColorAttachment;
        private RenderTargetHandle m_ColorAttachmentAfterOpaquePost;
        private RenderTargetHandle m_ColorAttachmentAfterTransparentPost;
        private RenderTargetHandle m_DepthAttachment;
        private RenderTargetHandle m_DepthTexture;
        private RenderTargetHandle m_OpaqueColor;
        private RenderTargetHandle m_MainLightShadowmap;
        private RenderTargetHandle m_AdditionalLightsShadowmap;
        private RenderTargetHandle m_ScreenSpaceShadowmap;

        private RenderTargetHandle[] m_WColorAttachments = new RenderTargetHandle[16];

        [NonSerialized]
        private bool m_Initialized = false;

        private void Init()
        {
            if (m_Initialized)
                return;

            m_DepthOnlyPass = new DepthOnlyPass();
            m_MainLightShadowCasterPass = new MainLightShadowCasterPass();
            m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass();
            m_SetupForwardRenderingPass = new SetupForwardRenderingPass();
            m_ScreenSpaceShadowResolvePass = new ScreenSpaceShadowResolvePass();
            m_CreateLightweightRenderTexturesPass = new CreateLightweightRenderTexturesPass();
            m_BeginXrRenderingPass = new BeginXRRenderingPass();
            m_SetupLightweightConstants = new SetupLightweightConstanstPass();
            m_RenderOpaqueForwardPass = new RenderOpaqueForwardPass();
            m_OpaquePostProcessPass = new PostProcessPass();
            m_DrawSkyboxPass = new DrawSkyboxPass();
            m_CopyDepthPass = new CopyDepthPass();
            m_CopyColorPass = new CopyColorPass();
            m_RenderTransparentForwardPass = new RenderTransparentForwardPass();
            m_RenderWaterColorProxyForwardPass = new RenderWaterColorProxyForwardPass();

            m_CreateLinearDepthPass = new CreateLinearDepthPass();
            m_QuadAdjustLoadPass = new QuadAdjustLoadPass();
            m_QuadBlendPass = new QuadBlendPass();
            m_QuadEdgeDetectionPass = new QuadEdgeDetectionPass();
            m_QuadEdgeManipulationPass = new QuadEdgeManipulationPass();
            m_QuadGapsOverlapsPass = new QuadGapsOverlapsPass();
            m_QuadPigmentApplicationPass = new QuadPigmentApplicationPass();
            m_QuadPigmentManipulationPass = new QuadPigmentManipulationPass();
            m_QuadSeparableHPass = new QuadSeparableHPass();
            m_QuadSeparableVPass = new QuadSeparableVPass();
            m_QuadSubstratePass = new QuadSubstratePass();
            m_QuadSubstrateLightingPass = new QuadSubstrateLightingPass();

            m_PostProcessPass = new PostProcessPass();
            m_FinalBlitPass = new FinalBlitPass();
            m_CapturePass = new CapturePass();
            m_EndXrRenderingPass = new EndXRRenderingPass();
            m_CreateWaterColorRenderTexturesPass = new CreateWaterColorRenderTexturesPass();



#if UNITY_EDITOR
            m_LitGizmoRenderingPass = new GizmoRenderingPass();
            m_UnlitGizmoRenderingPass = new GizmoRenderingPass();
            m_SceneViewDepthCopyPass = new SceneViewDepthCopyPass();
#endif


            // RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
            // Samples (MSAA) depend on camera and pipeline
            m_ColorAttachment.Init("_CameraColorTexture");//注册这个名字的shader变量
            m_ColorAttachmentAfterOpaquePost.Init("_CameraColorTextureAfterOpaquePost");
            m_ColorAttachmentAfterTransparentPost.Init("_CameraColorTextureAfterTransparentPost");
            m_DepthAttachment.Init("_CameraDepthAttachment");
            m_DepthTexture.Init("_CameraDepthTexture");
            m_OpaqueColor.Init("_CameraOpaqueTexture");
            m_MainLightShadowmap.Init("_MainLightShadowmapTexture");
            m_AdditionalLightsShadowmap.Init("_AdditionalLightsShadowmapTexture");
            m_ScreenSpaceShadowmap.Init("_ScreenSpaceShadowmapTexture");

            //new added
            //m_WColorAttachments[0].Init("_WCColorTarget");
            //m_WColorAttachments[1].Init("_WCDepthTarget");
            m_WColorAttachments[0] = RenderTargetHandle.CameraTarget;
            m_WColorAttachments[1] = RenderTargetHandle.CameraTarget;
            m_WColorAttachments[2].Init("_WCDiffuseTarget");
            m_WColorAttachments[3].Init("_WCSpecularTarget");
            m_WColorAttachments[4].Init("_WCPigmentCtrlTarget");
            m_WColorAttachments[5].Init("_WCSubstrateCtrlTarget");
            m_WColorAttachments[6].Init("_WCEdgeCtrlTarget");
            m_WColorAttachments[7].Init("_WCAbstractCtrlTarget");
            m_WColorAttachments[8].Init("_WCSubstrateTarget");
            m_WColorAttachments[9].Init("_WCLinearDepth");
            m_WColorAttachments[10].Init("_WCEdgeTarget_01");
            m_WColorAttachments[11].Init("_WCBleedingTarget_01");
            m_WColorAttachments[12].Init("_WCStylizationTarget_01");
            m_WColorAttachments[13].Init("_WCStylizationTarget_02");
            m_WColorAttachments[14].Init("_WCEdgeTarget_02");
            m_WColorAttachments[15].Init("_WCBleedingTarget_02");
            m_Initialized = true;
        }

        public static bool RequiresIntermediateColorTexture(ref RenderingData renderingData, RenderTextureDescriptor baseDescriptor)
        {
            CameraData cameraData = renderingData.cameraData;
            bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f);
            bool isTargetTexture2DArray = baseDescriptor.dimension == TextureDimension.Tex2DArray;
            bool requiresExplicitMsaaResolve = cameraData.msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve;
            bool isOffscreenRender = cameraData.camera.targetTexture != null && !cameraData.isSceneViewCamera;    
            bool isCapturing = cameraData.captureActions != null;

            bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve;
            if (isOffscreenRender)
                return requiresBlitForOffscreenCamera;

            return requiresBlitForOffscreenCamera || cameraData.isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
                isTargetTexture2DArray || !cameraData.isDefaultViewport || isCapturing || Display.main.requiresBlitToBackbuffer
                    || renderingData.killAlphaInFinalBlit;
        }

        List<IBeforeRender> m_BeforeRenderPasses = new List<IBeforeRender>(10);
        List<IAfterOpaquePass> m_AfterOpaquePasses = new List<IAfterOpaquePass>(10);
        List<IAfterOpaquePostProcess> m_AfterOpaquePostProcessPasses = new List<IAfterOpaquePostProcess>(10);
        List<IAfterSkyboxPass> m_AfterSkyboxPasses = new List<IAfterSkyboxPass>(10);
        List<IAfterTransparentPass> m_AfterTransparentPasses = new List<IAfterTransparentPass>(10);
        List<IAfterRender> m_AfterRenderPasses = new List<IAfterRender>(10);

        public void Setup(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            Init();//只在最初调用时执行一次

            Camera camera = renderingData.cameraData.camera;

            renderer.SetupPerObjectLightIndices(ref renderingData.cullResults, ref renderingData.lightData);
            //这边创建一个base description这样的话后面要创建RT的话直接修改这个base描述符就好了，使用方法如下面的shadow map
            RenderTextureDescriptor baseDescriptor = ScriptableRenderer.CreateRenderTextureDescriptor(ref renderingData.cameraData);
            ClearFlag clearFlag = ScriptableRenderer.GetCameraClearFlag(renderingData.cameraData.camera);
            RenderTextureDescriptor shadowDescriptor = baseDescriptor;
            shadowDescriptor.dimension = TextureDimension.Tex2D;

            bool mainLightShadows = false;
            if (renderingData.shadowData.supportsMainLightShadows)
            {
                mainLightShadows = m_MainLightShadowCasterPass.Setup(m_MainLightShadowmap, ref renderingData);
                if (mainLightShadows)
                    renderer.EnqueuePass(m_MainLightShadowCasterPass);
            }

            if (renderingData.shadowData.supportsAdditionalLightShadows)
            {
                bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(m_AdditionalLightsShadowmap, ref renderingData, renderer.maxVisibleAdditionalLights);
                if (additionalLightShadows)
                    renderer.EnqueuePass(m_AdditionalLightsShadowCasterPass);
            }

            bool resolveShadowsInScreenSpace = mainLightShadows && renderingData.shadowData.requiresScreenSpaceShadowResolve;
            // 首先处理阴影，然后再是其他的pass，这样符合了先后顺序
            // Depth prepass is generated in the following cases:
            // - We resolve shadows in screen space
            // - Scene view camera always requires a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
            // - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
            bool requiresDepthPrepass = resolveShadowsInScreenSpace ||
                                        renderingData.cameraData.isSceneViewCamera ||
                                        (renderingData.cameraData.requiresDepthTexture && (!CanCopyDepth(ref renderingData.cameraData)));

            // For now VR requires a depth prepass until we figure out how to properly resolve texture2DMS in stereo
            requiresDepthPrepass |= renderingData.cameraData.isStereoEnabled;

            renderer.EnqueuePass(m_SetupForwardRenderingPass);

            camera.GetComponents(m_BeforeRenderPasses);
            camera.GetComponents(m_AfterOpaquePasses);
            camera.GetComponents(m_AfterOpaquePostProcessPasses);
            camera.GetComponents(m_AfterSkyboxPasses);
            camera.GetComponents(m_AfterTransparentPasses);
            camera.GetComponents(m_AfterRenderPasses);

            bool createColorTexture = RequiresIntermediateColorTexture(ref renderingData, baseDescriptor)
                    || m_BeforeRenderPasses.Count != 0
                    || m_AfterOpaquePasses.Count != 0
                    || m_AfterOpaquePostProcessPasses.Count != 0
                    || m_AfterSkyboxPasses.Count != 0
                    || m_AfterTransparentPasses.Count != 0
                    || m_AfterRenderPasses.Count != 0
                    || true;// TODO: extract it!
            
            // If camera requires depth and there's no depth pre-pass we create a depth texture that can be read
            // later by effect requiring it. aha! 
            bool createDepthTexture = renderingData.cameraData.requiresDepthTexture && !requiresDepthPrepass || true;//TODO:  extract it!


            //#####create color and depth textures
            RenderTargetHandle colorHandle = (createColorTexture) ? m_ColorAttachment : RenderTargetHandle.CameraTarget;
            m_WColorAttachments[0] = colorHandle;
            RenderTargetHandle depthHandle = (createDepthTexture) ? m_DepthAttachment : RenderTargetHandle.CameraTarget;
            m_WColorAttachments[1] = depthHandle;

            var sampleCount = (SampleCount)renderingData.cameraData.msaaSamples;
            if (createColorTexture || createDepthTexture)
            {
                m_CreateLightweightRenderTexturesPass.Setup(baseDescriptor, colorHandle, depthHandle, sampleCount);
                renderer.EnqueuePass(m_CreateLightweightRenderTexturesPass);
            }

            if(true)
            {
                m_CreateWaterColorRenderTexturesPass.Setup(baseDescriptor, colorHandle, depthHandle, m_WColorAttachments, sampleCount);
                renderer.EnqueuePass(m_CreateWaterColorRenderTexturesPass);
            }

            foreach (var pass in m_BeforeRenderPasses)
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle, clearFlag));

            if (requiresDepthPrepass) //
            {
                m_DepthOnlyPass.Setup(baseDescriptor, m_DepthTexture);
                renderer.EnqueuePass(m_DepthOnlyPass);
            }

            if (resolveShadowsInScreenSpace)
            {
                m_ScreenSpaceShadowResolvePass.Setup(baseDescriptor, m_ScreenSpaceShadowmap);
                renderer.EnqueuePass(m_ScreenSpaceShadowResolvePass);
            }

            if (renderingData.cameraData.isStereoEnabled)
                renderer.EnqueuePass(m_BeginXrRenderingPass);

            var perObjectFlags = ScriptableRenderer.GetPerObjectLightFlags(renderingData.lightData.mainLightIndex, renderingData.lightData.additionalLightsCount);

            m_SetupLightweightConstants.Setup(renderer.maxVisibleAdditionalLights, renderer.perObjectLightIndices);
            renderer.EnqueuePass(m_SetupLightweightConstants);

            // If a before all render pass executed we expect it to clear the color render target
            if (m_BeforeRenderPasses.Count != 0)
                clearFlag = ClearFlag.None;

            m_RenderOpaqueForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, m_WColorAttachments, clearFlag, camera.backgroundColor, perObjectFlags);
            renderer.EnqueuePass(m_RenderOpaqueForwardPass);
            foreach (var pass in m_AfterOpaquePasses)
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

            if (renderingData.cameraData.postProcessEnabled &&
                renderingData.cameraData.postProcessLayer.HasOpaqueOnlyEffects(renderer.postProcessingContext))
            {
                m_OpaquePostProcessPass.Setup(baseDescriptor, colorHandle, colorHandle, true, false);
                renderer.EnqueuePass(m_OpaquePostProcessPass);

                foreach (var pass in m_AfterOpaquePostProcessPasses)
                    renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));
            }

            if (camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
            {
                // We can't combine skybox and render opaques passes if there's a custom render pass in between
                // them. Ideally we need a render graph here that each render pass declares inputs and output
                // attachments and their Load/Store action so we figure out properly if we can combine passes
                // and move to interleaved rendering with RenderPass API. 
                bool combineWithRenderOpaquesPass = m_AfterOpaquePostProcessPasses.Count == 0;
                m_DrawSkyboxPass.Setup(baseDescriptor, colorHandle, depthHandle, combineWithRenderOpaquesPass);
                renderer.EnqueuePass(m_DrawSkyboxPass);
            }

            foreach (var pass in m_AfterSkyboxPasses)
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

            // If a depth texture was created we necessarily need to copy it, otherwise we could have render it to a renderbuffer
            if (true)
            {
                m_CreateLinearDepthPass.Setup(depthHandle, m_WColorAttachments[9]);// m_DepthTexture, m_WColorAttachments[9]);
                renderer.EnqueuePass(m_CreateLinearDepthPass);
            }

            if (false)// createDepthTexture
            {
                //m_CopyDepthPass.Setup(depthHandle, m_DepthTexture);// m_DepthTexture, m_WColorAttachments[9]);
                //renderer.EnqueuePass(m_CopyDepthPass);
            }
            // will not enter this 
            if (renderingData.cameraData.requiresOpaqueTexture)
            {
                m_CopyColorPass.Setup(colorHandle, m_OpaqueColor);
                renderer.EnqueuePass(m_CopyColorPass);
            }

            m_RenderWaterColorProxyForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, m_WColorAttachments, perObjectFlags);
            renderer.EnqueuePass(m_RenderWaterColorProxyForwardPass);

            m_RenderTransparentForwardPass.Setup(baseDescriptor, colorHandle, depthHandle, perObjectFlags);
            renderer.EnqueuePass(m_RenderTransparentForwardPass);

            foreach (var pass in m_AfterTransparentPasses)
                renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

#if UNITY_EDITOR
            m_LitGizmoRenderingPass.Setup(true);
            renderer.EnqueuePass(m_LitGizmoRenderingPass);
#endif

            // TODO: Add Water Color Post Process Here
            /*
            m_WColorAttachments[2].Init("_WCDiffuseTarget");
            m_WColorAttachments[3].Init("_WCSpecularTarget");
            m_WColorAttachments[4].Init("_WCPigmentCtrlTarget");
            m_WColorAttachments[5].Init("_WCSubstrateCtrlTarget");
            m_WColorAttachments[6].Init("_WCEdgeCtrlTarget");
            m_WColorAttachments[7].Init("_WCAbstractCtrlTarget");
            m_WColorAttachments[8].Init("_WCSubstrateTarget");
            m_WColorAttachments[9].Init("_WCLinearDepth");
            m_WColorAttachments[10].Init("_WCEdgeTarget");
            m_WColorAttachments[11].Init("_WCBleedingTarget");
            m_WColorAttachments[12].Init("_WCStylizationTarget");
             */
            int id = 12;
            if (true)// it's enable WATER COLOR
            {
                
                m_QuadAdjustLoadPass.Setup(baseDescriptor, colorHandle, depthHandle, m_WColorAttachments);
                renderer.EnqueuePass(m_QuadAdjustLoadPass);
                m_QuadEdgeDetectionPass.Setup(baseDescriptor, m_WColorAttachments[id], m_WColorAttachments[9],m_WColorAttachments[10]);
                renderer.EnqueuePass(m_QuadEdgeDetectionPass);
                m_QuadPigmentManipulationPass.Setup(baseDescriptor, m_WColorAttachments[id], m_WColorAttachments[4], m_WColorAttachments[13],ref id);
                renderer.EnqueuePass(m_QuadPigmentManipulationPass);
                m_QuadSeparableHPass.Setup(baseDescriptor, depthHandle, m_WColorAttachments, id);
                renderer.EnqueuePass(m_QuadSeparableHPass);
                m_QuadSeparableVPass.Setup(baseDescriptor, depthHandle, m_WColorAttachments, id);
                renderer.EnqueuePass(m_QuadSeparableVPass);

                m_QuadBlendPass.Setup(baseDescriptor, m_WColorAttachments[id], m_WColorAttachments[15], m_WColorAttachments[12], ref id);
                renderer.EnqueuePass(m_QuadBlendPass);

                m_QuadEdgeManipulationPass.Setup(baseDescriptor, m_WColorAttachments, ref id);
                renderer.EnqueuePass(m_QuadEdgeManipulationPass);

                m_QuadGapsOverlapsPass.Setup(baseDescriptor, m_WColorAttachments, ref id);
                renderer.EnqueuePass(m_QuadGapsOverlapsPass);

                m_QuadPigmentApplicationPass.Setup(baseDescriptor, m_WColorAttachments, ref id);
                renderer.EnqueuePass(m_QuadPigmentApplicationPass);

                m_QuadSubstratePass.Setup(baseDescriptor, m_WColorAttachments, ref id);
                renderer.EnqueuePass(m_QuadSubstratePass);

                m_QuadSubstrateLightingPass.Setup(baseDescriptor, m_WColorAttachments, ref id);
                renderer.EnqueuePass(m_QuadSubstrateLightingPass);
            }

            
            bool afterRenderExists = m_AfterRenderPasses.Count != 0;

            colorHandle = m_WColorAttachments[id];
            // if we have additional filters
            // we need to stay in a RT
            if (afterRenderExists)
            {
                // perform post with src / dest the same
                if (renderingData.cameraData.postProcessEnabled)
                {
                    //Debug.Log(1);
                    m_PostProcessPass.Setup(baseDescriptor, colorHandle, colorHandle, false, false);
                    renderer.EnqueuePass(m_PostProcessPass);
                }

                //execute after passes
                foreach (var pass in m_AfterRenderPasses)
                    renderer.EnqueuePass(pass.GetPassToEnqueue(baseDescriptor, colorHandle, depthHandle));

                //now blit into the final target
                if (colorHandle != RenderTargetHandle.CameraTarget)
                {
                    //Debug.Log(2);
                    if (m_CapturePass.Setup(colorHandle, renderingData.cameraData.captureActions))
                        renderer.EnqueuePass(m_CapturePass);

                    m_FinalBlitPass.Setup(baseDescriptor, colorHandle, Display.main.requiresSrgbBlitToBackbuffer, renderingData.killAlphaInFinalBlit);
                    //m_FinalBlitPass.Setup(baseDescriptor, m_WColorAttachments[12], Display.main.requiresSrgbBlitToBackbuffer, renderingData.killAlphaInFinalBlit);
                    renderer.EnqueuePass(m_FinalBlitPass);
                }
            }
            else
            {
                if (renderingData.cameraData.postProcessEnabled)
                {
                    //Debug.Log(3);
                    m_PostProcessPass.Setup(baseDescriptor, colorHandle, RenderTargetHandle.CameraTarget, false, renderingData.cameraData.camera.targetTexture == null);
                    renderer.EnqueuePass(m_PostProcessPass);
                }
                else if (colorHandle != RenderTargetHandle.CameraTarget)
                {
                    //Debug.Log(4);
                    if (m_CapturePass.Setup(colorHandle, renderingData.cameraData.captureActions))
                        renderer.EnqueuePass(m_CapturePass);

                    m_FinalBlitPass.Setup(baseDescriptor, colorHandle, Display.main.requiresSrgbBlitToBackbuffer, renderingData.killAlphaInFinalBlit);
                    renderer.EnqueuePass(m_FinalBlitPass);
                }
            }

            //if (true)
            //{
            //    m_QuadSubstrateLightingPass.Setup(baseDescriptor, m_WColorAttachments, ref id);
            //    renderer.EnqueuePass(m_QuadSubstrateLightingPass);
            //}
            

            if (renderingData.cameraData.isStereoEnabled)
            {
                renderer.EnqueuePass(m_EndXrRenderingPass);
            }


#if UNITY_EDITOR
            m_UnlitGizmoRenderingPass.Setup(false);
            renderer.EnqueuePass(m_UnlitGizmoRenderingPass);
            if (renderingData.cameraData.isSceneViewCamera)//
            {
                m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
                renderer.EnqueuePass(m_SceneViewDepthCopyPass);
            }
#endif

        }

        bool CanCopyDepth(ref CameraData cameraData)
        {
            bool msaaEnabledForCamera = (int)cameraData.msaaSamples > 1;
            bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
            bool supportsDepthTarget = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
            bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

            // TODO:  We don't have support to high p Texture2DMS currently and this breaks depth precision.
            // currently disabling it until shader changes kick in.
            //bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
            bool msaaDepthResolve = false;
            return supportsDepthCopy || msaaDepthResolve;
        }
    }
}
