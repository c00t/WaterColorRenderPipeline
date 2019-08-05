using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace UnityEngine.Experimental.Rendering.LWRP
{
    /// <summary>
    /// Copy the given depth buffer into the given destination depth buffer.
    /// 
    /// You can use this pass to copy a depth buffer to a destination,
    /// so you can use it later in rendering. If the source texture has MSAA
    /// enabled, the pass uses a custom MSAA resolve. If the source texture
    /// does not have MSAA enabled, the pass uses a Blit or a Copy Texture
    /// operation, depending on what the current platform supports.
    /// </summary>
    internal class CreateLinearDepthPass : ScriptableRenderPass
    {
        private RenderTargetHandle source { get; set; }
        private RenderTargetHandle destination { get; set; }
        const string k_CreateLinearDepthTag = "Create Linear Depth";

        /// <summary>
        /// Configure the pass with the source and destination to execute on.
        /// </summary>
        /// <param name="source">Source Render Target</param>
        /// <param name="destination">Destination Render Targt</param>
        public void Setup(RenderTargetHandle source, RenderTargetHandle destination)
        {
            this.source = source;
            this.destination = destination;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            CommandBuffer cmd = CommandBufferPool.Get(k_CreateLinearDepthTag);
            RenderTargetIdentifier depthSurface = source.Identifier();
            RenderTargetIdentifier linearDepthSurface = destination.Identifier();
            //因为是一个全局的方法，所以必须新建一个mat
            //Material depthCopyMaterial = ;

            //RenderTextureDescriptor descriptor = ScriptableRenderer.CreateRenderTextureDescriptor(ref renderingData.cameraData);
            //descriptor.colorFormat = RenderTextureFormat.Depth;
            //descriptor.depthBufferBits = 32; //TODO: fix this ;
            //descriptor.msaaSamples = 1;
            //descriptor.bindMS = false;
            //cmd.GetTemporaryRT(destination.id, descriptor, FilterMode.Point);
            //但是其实这个depth就已经可以直接使用了，没必要在后面再搞一个depth texture
            cmd.SetGlobalTexture("_CameraDepthAttachment", source.Identifier());
            cmd.SetGlobalTexture("_gLinearDepthTex", destination.Identifier());
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
               
            }
            else
            {
                cmd.EnableShaderKeyword(ShaderKeywordStrings.DepthNoMsaa);
                cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa2);
                cmd.DisableShaderKeyword(ShaderKeywordStrings.DepthMsaa4);
                // the same with above implement
                //ScriptableRenderer.CopyTexture(cmd, depthSurface, copyDepthSurface, depthCopyMaterial);
            }
            // they are the same in current version
            Material linearDepth = renderer.GetMaterial(MaterialHandle.CreateLinearDepth);
            linearDepth.SetVector("_gDepthRange", renderingData.watercolorData.depthRange);
            linearDepth.SetVector("_gAtmosphereRange", renderingData.watercolorData.atmosphereRange);
            cmd.Blit(depthSurface, linearDepthSurface, linearDepth);
            context.ExecuteCommandBuffer(cmd);
            //release 之后，这个还存在吗
            //TODO: 其实可以在之后测试一下;
            CommandBufferPool.Release(cmd);
        }
        
        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            
            if (destination != RenderTargetHandle.CameraTarget)
            {
                //cmd.ReleaseTemporaryRT(destination.id);
                //destination = RenderTargetHandle.CameraTarget;
            }
        }
    }
}
