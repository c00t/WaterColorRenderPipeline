using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace UnityEngine.Experimental.Rendering.LWRP
{
    /// <summary>
    /// Copy the given color target to the current camera target
    ///
    /// You can use this pass to copy the result of rendering to
    /// the camera target. The pass takes the screen viewport into
    /// consideration.
    /// </summary>
    internal class QuadAdjustLoadPass : ScriptableRenderPass
    {
        const string k_AdjustLoadTag = "Quad Adjust Load Pass";

        private RenderTargetHandle colorAttachmentHandle { get; set; }
        private RenderTargetHandle depthAttachmentHandle { get; set; }
        private RenderTargetHandle[] inHandles = new RenderTargetHandle[3];
        private RenderTargetIdentifier[] outBuffers = new RenderTargetIdentifier[2];
        private RenderTextureDescriptor descriptor { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorAttachmentHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle colorAttachmentHandle, RenderTargetHandle depthAttachmentHandle, RenderTargetHandle[] outHandles)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.descriptor = baseDescriptor;
            this.depthAttachmentHandle = depthAttachmentHandle;
            //in rendertarget handles
            inHandles[0] = outHandles[2];
            inHandles[1] = outHandles[3];
            inHandles[2] = outHandles[9];
            //out render target is <"stylizationTarget", "substrateTarget", "linearDepth", "velocity">
            outBuffers[0] = outHandles[12].Identifier();
            outBuffers[1] = outHandles[8].Identifier();
            //outBuffers[2] = outHandles[9].Identifier(); //TODO: specify it's useful???
            //velocity is useless
            //this.outHandles = outHandles;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            CommandBuffer cmd = CommandBufferPool.Get(k_AdjustLoadTag);

            //if (requiresSRGConversion)
            //    cmd.EnableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);
            //else
            //    cmd.DisableShaderKeyword(ShaderKeywordStrings.LinearToSRGBConversion);

            //if (killAlpha)
            //    cmd.EnableShaderKeyword(ShaderKeywordStrings.KillAlpha);
            //else
            //    cmd.DisableShaderKeyword(ShaderKeywordStrings.KillAlpha);

            //if (renderingData.cameraData.isStereoEnabled || renderingData.cameraData.isSceneViewCamera)
            //{
            //    cmd.Blit(colorAttachmentHandle.Identifier(), BuiltinRenderTextureType.CameraTarget);
            //}
            //else
            //{
            if (renderingData.cameraData.msaaSamples > 1)
            {
                cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthNoMsaa);
                if (renderingData.cameraData.msaaSamples == 4)
                {
                    cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                    cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                }
                else
                {
                    cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                    cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                }
                //cmd.Blit(depthSurface, copyDepthSurface, depthCopyMaterial);
            }
            else
            {
                cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthNoMsaa);
                cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                // the same with above implement
                //ScriptableRenderer.CopyTexture(cmd, depthSurface, copyDepthSurface, depthCopyMaterial);
            }

            Material adjustload = renderer.GetMaterial(MaterialHandle.AdjustLoad);
            
            adjustload.SetTexture("_gSubstrateTex", renderer.GetSubstrateBack());
            adjustload.SetFloat("_gSaturation", renderingData.watercolorData.postprocessSaturation);
            adjustload.SetFloat("_gContrast", renderingData.watercolorData.postprocessContrast);
            adjustload.SetFloat("_gBrightness", renderingData.watercolorData.postprocessBrightness);
            adjustload.SetFloat("_gSubstrateTexScale", renderingData.watercolorData.substrateScale);
            adjustload.SetFloat("_gSubstrateRoughness", renderingData.watercolorData.substrateRoughness);
            adjustload.SetVector("_gDepthRange", renderingData.watercolorData.depthRange);
            adjustload.SetVector("_gAtmosphereRange", renderingData.watercolorData.atmosphereRange);
            adjustload.SetColor("_gSubstrateColor", renderingData.watercolorData.substrateColor);
            adjustload.SetColor("_gAtmosphereTint", renderingData.watercolorData.atmosphereTint);
            //adjustload.SetTexture("_gColorTex", colorAttachmentHandle.Identifier());
            //adjustload.SetTexture
            //cmd.SetGlobalTexture("_gZBuffer", depthAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gColorTex", colorAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gDiffuseTex", inHandles[0].Identifier());
            cmd.SetGlobalTexture("_gSpecularTex", inHandles[1].Identifier());
            cmd.SetGlobalTexture("_gLinearDepthTex", inHandles[2].Identifier());

            SetRenderTarget(cmd, outBuffers, depthAttachmentHandle.Identifier(), ClearFlag.None, Color.black);
            //cmd.SetGlobalTexture("_BlitTex", colorAttachmentHandle.Identifier());//must use this func to uwse the txeture in unity

            //SetRenderTarget(
            //    cmd,
            //    BuiltinRenderTextureType.CameraTarget,
            //    RenderBufferLoadAction.DontCare,
            //    RenderBufferStoreAction.Store,
            //    ClearFlag.None,
            //    Color.black,
            //    descriptor.dimension);

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(new Rect(0f, 0f, renderingData.cameraData.camera.pixelRect.width * renderingData.cameraData.renderScale, renderingData.cameraData.camera.pixelRect.height * renderingData.cameraData.renderScale));
            //renderingData.cameraData.camera
            /*
            Matrix4x4 projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            Matrix4x4 viewMatrix = camera.worldToCameraMatrix;
            Matrix4x4 viewProjMatrix = projMatrix * viewMatrix;
            Matrix4x4 invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
            Shader.SetGlobalMatrix(PerCameraBuffer._InvCameraViewProj, invViewProjMatrix);
             */
            ScriptableRenderer.RenderFullscreenQuad(cmd, adjustload);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
