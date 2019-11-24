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
    internal class QuadBlendPass : ScriptableRenderPass
    {
        const string k_BlendTag = "Quad Blend Pass";

        private RenderTargetHandle stylizationHandle { get; set; }
        private RenderTargetHandle bleedingHandle { get; set; }
        private RenderTargetHandle outHandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorAttachmentHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle stylizationHandle, RenderTargetHandle bleedingHandle, RenderTargetHandle outHandle,ref int id)
        {
            this.stylizationHandle = stylizationHandle;
            this.descriptor = baseDescriptor;
            this.bleedingHandle = bleedingHandle;
            this.outHandle = outHandle;
            id = 12;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            CommandBuffer cmd = CommandBufferPool.Get(k_BlendTag);

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
            Material blend = renderer.GetMaterial(MaterialHandle.Blend);
            //adjustload.SetTexture("_gSubstrateTex", renderer.GetSubstrateBack());
            //adjustload.SetTexture("_gColorTex", colorAttachmentHandle.Identifier());
            //adjustload.SetTexture
            //cmd.SetGlobalTexture("_gZBuffer", depthAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gColorTex", stylizationHandle.Identifier());
            cmd.SetGlobalTexture("_gBlendTex", bleedingHandle.Identifier());

            //cmd.SetGlobalTexture("_BlitTex", colorAttachmentHandle.Identifier());//must use this func to uwse the txeture in unity

            SetRenderTarget(
                cmd,
                outHandle.Identifier(),
                RenderBufferLoadAction.DontCare,
                RenderBufferStoreAction.Store,
                ClearFlag.None,
                Color.black,
                descriptor.dimension);

            cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            cmd.SetViewport(new Rect(0f, 0f, renderingData.cameraData.camera.pixelRect.width * renderingData.cameraData.renderScale, renderingData.cameraData.camera.pixelRect.height * renderingData.cameraData.renderScale));
            ScriptableRenderer.RenderFullscreenQuad(cmd, blend);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
