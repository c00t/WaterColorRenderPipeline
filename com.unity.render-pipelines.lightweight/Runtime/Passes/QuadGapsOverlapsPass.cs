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
    internal class QuadGapsOverlapsPass : ScriptableRenderPass
    {
        const string k_GapsOverlapsTag = "Quad GapsOverLaps Pass";
        private RenderTargetHandle[] inHandles = new RenderTargetHandle[4];
        private RenderTargetHandle outHandle { get; set; }
        private RenderTextureDescriptor descriptor { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        /// <param name="baseDescriptor"></param>
        /// <param name="colorAttachmentHandle"></param>
        public void Setup(RenderTextureDescriptor baseDescriptor, RenderTargetHandle[] outHandles,ref int id)
        {

            this.descriptor = baseDescriptor;
            inHandles[0] = outHandles[id];
            inHandles[1] = outHandles[10];
            inHandles[2] = outHandles[6];
            inHandles[3] = outHandles[15];


            this.outHandle = outHandles[12];
            id = 12;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));

            CommandBuffer cmd = CommandBufferPool.Get(k_GapsOverlapsTag);

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
            Material gapsOverlaps = renderer.GetMaterial(MaterialHandle.GapsOverlaps);
            gapsOverlaps.SetColor("_gSubstrateColor", renderingData.watercolorData.substrateColor);
            gapsOverlaps.SetFloat("_gGORadius", renderingData.watercolorData.gapsOverlapsWidth);
            //adjustload.SetTexture("_gSubstrateTex", renderer.GetSubstrateBack());
            //adjustload.SetTexture("_gColorTex", colorAttachmentHandle.Identifier());
            //adjustload.SetTexture
            //cmd.SetGlobalTexture("_gZBuffer", depthAttachmentHandle.Identifier());
            cmd.SetGlobalTexture("_gColorTex", inHandles[0].Identifier());
            cmd.SetGlobalTexture("_gEdgeTex", inHandles[1].Identifier());
            cmd.SetGlobalTexture("_gControlTex", inHandles[2].Identifier());
            cmd.SetGlobalTexture("_gBlendingTex", inHandles[3].Identifier());
            //cmd.SetGlobalTexture("_gLinearDepthTex", colorAttachmentHandle.Identifier());

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
            cmd.SetViewport(renderingData.cameraData.camera.pixelRect);
            ScriptableRenderer.RenderFullscreenQuad(cmd, gapsOverlaps);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
