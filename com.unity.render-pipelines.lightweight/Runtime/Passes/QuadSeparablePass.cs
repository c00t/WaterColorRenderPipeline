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
    internal class QuadSeparableHPass : ScriptableRenderPass
    {
        const string k_SeparableTag = "Quad Separable H Pass";

        private RenderTargetHandle[] inHandles = new RenderTargetHandle[5];
        private RenderTargetIdentifier[] outBuffers = new RenderTargetIdentifier[2];
        private RenderTargetHandle depthhandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }
        private float[] weights = new float[161];

        public QuadSeparableHPass()
        {
            int bleedingRadius = 10;
            float sigma = (float)bleedingRadius * 2.0f;
            float normDivisor = 0;
            for (int x = - bleedingRadius; x <= bleedingRadius; x++)
            {
                
                float weight = (float)(0.15915 * Math.Exp(-0.5 * x * x / (sigma * sigma)) / sigma);
                //float weight = (float)(pow((6.283185*sigma*sigma), -0.5) * exp((-0.5*x*x) / (sigma*sigma)));
                normDivisor += weight;
                weights[x + bleedingRadius] = weight;
            }
            for (int x = -bleedingRadius; x <= bleedingRadius; x++)
            {
                weights[x + bleedingRadius] /= normDivisor;
            }
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorAttachmentHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor,RenderTargetHandle depthhandle,RenderTargetHandle[] outHandles,int id)
        {
            this.descriptor = baseDescriptor;
            this.depthhandle = depthhandle;
            inHandles[0] = outHandles[id];//stylization
            inHandles[1] = outHandles[10]; // use edge target 1
            inHandles[2] = outHandles[9];
            inHandles[3] = outHandles[6];
            inHandles[4] = outHandles[7];

            outBuffers[0] = outHandles[11].Identifier();
            outBuffers[1] = outHandles[14].Identifier();
            //this.outHandles = outHandles;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            CommandBuffer cmd = CommandBufferPool.Get(k_SeparableTag);

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
            Material separable = renderer.GetMaterial(MaterialHandle.Separable);
            separable.SetFloat("_gRenderScale", renderingData.watercolorData.substrateScale);
            separable.SetFloat("_gBleedingThreshold", renderingData.watercolorData.bleedingThreshold);
            separable.SetFloat("_gEdgeDarkeningKernel", renderingData.watercolorData.edgeDarkenWidth);
            separable.SetFloat("_gGapsOverlapsKernel", renderingData.watercolorData.gapsOverlapsWidth);
            cmd.SetGlobalFloatArray("_gGaussianWeights", weights);
            //adjustload.SetTexture("_gSubstrateTex", renderer.GetSubstrateBack());
            //adjustload.SetTexture("_gColorTex", colorAttachmentHandle.Identifier());
            //adjustload.SetTexture
            //cmd.SetGlobalTexture("_gZBuffer", depthAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gColorTex", inHandles[0].Identifier());
            cmd.SetGlobalTexture("_gEdgeTex", inHandles[1].Identifier());
            cmd.SetGlobalTexture("_gDepthTex", inHandles[2].Identifier());
            cmd.SetGlobalTexture("_gEdgeControlTex", inHandles[3].Identifier());
            cmd.SetGlobalTexture("_gAbstractionControlTex", inHandles[4].Identifier());
            //cmd.SetGlobalTexture("_gLinearDepthTex", colorAttachmentHandle.Identifier());

            //cmd.SetGlobalTexture("_BlitTex", colorAttachmentHandle.Identifier());//must use this func to uwse the txeture in unity
            SetRenderTarget(cmd, outBuffers, depthhandle.Identifier(), ClearFlag.None, Color.black);

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
            ScriptableRenderer.RenderFullscreenQuad(cmd, separable,null,0);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    internal class QuadSeparableVPass : ScriptableRenderPass
    {
        const string k_SeparableTag = "Quad Separable V Pass";

        private RenderTargetHandle[] inHandles = new RenderTargetHandle[5];
        private RenderTargetIdentifier[] outBuffers = new RenderTargetIdentifier[2];
        private RenderTargetHandle depthhandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }
        private float[] weights = new float[161];

        public QuadSeparableVPass()
        {
            int bleedingRadius = 10;
            float sigma = (float)bleedingRadius * 2.0f;
            float normDivisor = 0;
            for (int x = -bleedingRadius; x <= bleedingRadius; x++)
            {

                float weight = (float)(0.15915 * Math.Exp(-0.5 * x * x / (sigma * sigma)) / sigma);
                //float weight = (float)(pow((6.283185*sigma*sigma), -0.5) * exp((-0.5*x*x) / (sigma*sigma)));
                normDivisor += weight;
                weights[x + bleedingRadius] = weight;
            }
            for (int x = -bleedingRadius; x <= bleedingRadius; x++)
            {
                weights[x + bleedingRadius] /= normDivisor;
            }
        }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorAttachmentHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle depthhandle, RenderTargetHandle[] outHandles, int id)
        {
            this.descriptor = baseDescriptor;
            this.depthhandle = depthhandle;
            inHandles[0] = outHandles[11];//stylization
            inHandles[1] = outHandles[14]; // use edge target 2
            inHandles[2] = outHandles[9];
            inHandles[3] = outHandles[6];
            inHandles[4] = outHandles[7];

            outBuffers[0] = outHandles[15].Identifier();
            outBuffers[1] = outHandles[10].Identifier();
            //this.outHandles = outHandles;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            CommandBuffer cmd = CommandBufferPool.Get(k_SeparableTag);

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
                // the same with above implemnet
                //ScriptableRenderer.CopyTexture(cmd, depthSurface, copyDepthSurface, depthCopyMaterial);
            }
            Material separable = renderer.GetMaterial(MaterialHandle.Separable);
            cmd.SetGlobalFloatArray("_gGaussianWeights", weights);
            //adjustload.SetTexture("_gSubstrateTex", renderer.GetSubstrateBack());
            //adjustload.SetTexture("_gColorTex", colorAttachmentHandle.Identifier());
            //adjustload.SetTexture
            //cmd.SetGlobalTexture("_gZBuffer", depthAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gColorTex", inHandles[0].Identifier());
            cmd.SetGlobalTexture("_gEdgeTex", inHandles[1].Identifier());
            cmd.SetGlobalTexture("_gDepthTex", inHandles[2].Identifier());
            cmd.SetGlobalTexture("_gEdgeControlTex", inHandles[3].Identifier());
            cmd.SetGlobalTexture("_gAbstractionControlTex", inHandles[4].Identifier());
            //cmd.SetGlobalTexture("_gLinearDepthTex", colorAttachmentHandle.Identifier());

            //cmd.SetGlobalTexture("_BlitTex", colorAttachmentHandle.Identifier());//must use this func to uwse the txeture in unity
            SetRenderTarget(cmd, outBuffers, depthhandle.Identifier(), ClearFlag.None, Color.black);

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
            ScriptableRenderer.RenderFullscreenQuad(cmd, separable, null, 1);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
