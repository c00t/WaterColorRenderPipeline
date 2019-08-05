using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace UnityEngine.Experimental.Rendering.LWRP
{
    /// <summary>
    /// Generate rendering attachments that can be used for rendering.
    ///
    /// You can use this pass to generate valid rendering targets that
    /// the Lightweight Render Pipeline can use for rendering. For example,
    /// when you render a frame, the LWRP renders into a valid color and
    /// depth buffer.
    /// </summary>
    internal class CreateLightweightRenderTexturesPass : ScriptableRenderPass
    {
        const string k_CreateRenderTexturesTag = "Create Render Textures";
        const int k_DepthStencilBufferBits = 32;
        private RenderTargetHandle colorAttachmentHandle { get; set; }
        private RenderTargetHandle depthAttachmentHandle { get; set; }
        private RenderTextureDescriptor descriptor;
        private SampleCount samples { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle,
            SampleCount samples)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.depthAttachmentHandle = depthAttachmentHandle;
            this.samples = samples;
            descriptor = baseDescriptor;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");

            CommandBuffer cmd = CommandBufferPool.Get(k_CreateRenderTexturesTag);
            //注意任何用command buffer 执行的东西到了最后都是全局的
            if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                //Debug.Log(1);
                bool useDepthRenderBuffer = depthAttachmentHandle == RenderTargetHandle.CameraTarget;
                var colorDescriptor = descriptor;
                colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
                cmd.GetTemporaryRT(colorAttachmentHandle.id, colorDescriptor, FilterMode.Bilinear);
            }

            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                //Debug.Log(2);
                var depthDescriptor = descriptor;
                depthDescriptor.colorFormat = RenderTextureFormat.Depth;
                depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
                depthDescriptor.bindMS = (int)samples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures!=0);
                cmd.GetTemporaryRT(depthAttachmentHandle.id, depthDescriptor, FilterMode.Point);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            
            if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(colorAttachmentHandle.id);
                colorAttachmentHandle = RenderTargetHandle.CameraTarget;
            }

            if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            {
                cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
                depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            }
        }
    }
}