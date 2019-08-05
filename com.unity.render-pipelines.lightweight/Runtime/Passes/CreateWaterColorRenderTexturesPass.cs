using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Experimental.Rendering;

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
    internal class CreateWaterColorRenderTexturesPass : ScriptableRenderPass
    {
        const string k_CreateWaterColorRenderTexturesTag = "Create Water Color Render Textures";
        const int k_DepthStencilBufferBits = 32;
        private RenderTargetHandle colorAttachmentHandle { get; set; }
        private RenderTargetHandle depthAttachmentHandle { get; set; }
        private RenderTextureDescriptor descriptor;
        //private RenderTextureDescriptor ctrlDescriptor { get; set; }
        //private RenderTextureDescriptor depthDescriptor { get; set; }
        //private RenderTextureDescriptor subDescriptor { get; set; }

        private RenderTargetHandle[] watercolorHandles { get; set; }
        private SampleCount samples { get; set; }

        /// <summary>
        /// Configure the pass
        /// </summary>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle,
            RenderTargetHandle[] watercolorHandles,
            SampleCount samples)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.depthAttachmentHandle = depthAttachmentHandle;
            this.watercolorHandles = watercolorHandles;
            this.samples = samples;
            descriptor = baseDescriptor;
            //baseDescriptor.colorFormat = RenderTextureFormat.ARGB32;
            //ctrlDescriptor = baseDescriptor;
            //baseDescriptor.colorFormat = RenderTextureFormat.RGFloat;
            //depthDescriptor = baseDescriptor;
            //baseDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
            //subDescriptor = baseDescriptor;
            //ctrlDescriptor.colorFormat = RenderTextureFormat.ARGB32;
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");
            CommandBuffer cmd = CommandBufferPool.Get(k_CreateWaterColorRenderTexturesTag);
            var commonDescriptor = descriptor;
            var width = descriptor.width;
            var height = descriptor.height;
            commonDescriptor.depthBufferBits = 0;
            //0 and 1 is  
            cmd.GetTemporaryRT(watercolorHandles[2].id, commonDescriptor, FilterMode.Bilinear);//diffuse
            cmd.GetTemporaryRT(watercolorHandles[12].id, commonDescriptor, FilterMode.Bilinear);//stylization
            cmd.GetTemporaryRT(watercolorHandles[13].id, commonDescriptor, FilterMode.Bilinear);
            commonDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;//we can pack and unpack something to this half method.them v-ram can be saved.
            cmd.GetTemporaryRT(watercolorHandles[3].id, commonDescriptor, FilterMode.Bilinear);//specular
            cmd.GetTemporaryRT(watercolorHandles[4].id, commonDescriptor, FilterMode.Bilinear);//pigment ctrl
            cmd.GetTemporaryRT(watercolorHandles[5].id, commonDescriptor, FilterMode.Bilinear);//substrate ctrl
            cmd.GetTemporaryRT(watercolorHandles[6].id, commonDescriptor, FilterMode.Bilinear);//edge ctrl
            cmd.GetTemporaryRT(watercolorHandles[7].id, commonDescriptor, FilterMode.Bilinear);//abstract ctrl
            cmd.GetTemporaryRT(watercolorHandles[10].id, commonDescriptor, FilterMode.Bilinear);//edge target
            cmd.GetTemporaryRT(watercolorHandles[14].id, commonDescriptor, FilterMode.Bilinear);//edge target
            cmd.GetTemporaryRT(watercolorHandles[11].id, commonDescriptor, FilterMode.Bilinear);//bleeding target
            cmd.GetTemporaryRT(watercolorHandles[15].id, commonDescriptor, FilterMode.Bilinear);
            commonDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
            cmd.GetTemporaryRT(watercolorHandles[8].id, commonDescriptor, FilterMode.Bilinear); //SubstrateTarget
            commonDescriptor.colorFormat = RenderTextureFormat.RGFloat;
            cmd.GetTemporaryRT(watercolorHandles[9].id, commonDescriptor, FilterMode.Point);// linear depth
            //注意任何用command buffer 执行的东西到了最后都是全局的
            //if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            //{
            //    bool useDepthRenderBuffer = depthAttachmentHandle == RenderTargetHandle.CameraTarget;
            //    var colorDescriptor = descriptor;
            //    colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
            //    cmd.GetTemporaryRT(colorAttachmentHandle.id, colorDescriptor, FilterMode.Bilinear);
            //}

            //if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            //{
            //    var depthDescriptor = descriptor;
            //    depthDescriptor.colorFormat = RenderTextureFormat.Depth;
            //    depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
            //    depthDescriptor.bindMS = (int)samples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures!=0);
            //    cmd.GetTemporaryRT(depthAttachmentHandle.id, depthDescriptor, FilterMode.Point);
            //}

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <inheritdoc/>
        public override void FrameCleanup(CommandBuffer cmd)
        {
            if (cmd == null)
                throw new ArgumentNullException("cmd");
            cmd.ReleaseTemporaryRT(watercolorHandles[2].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[3].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[4].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[5].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[6].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[7].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[8].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[9].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[10].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[11].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[12].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[13].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[14].id);
            cmd.ReleaseTemporaryRT(watercolorHandles[15].id);
            //if (colorAttachmentHandle != RenderTargetHandle.CameraTarget)
            //{
            //    cmd.ReleaseTemporaryRT(colorAttachmentHandle.id);
            //    colorAttachmentHandle = RenderTargetHandle.CameraTarget;
            //}

            //if (depthAttachmentHandle != RenderTargetHandle.CameraTarget)
            //{
            //    cmd.ReleaseTemporaryRT(depthAttachmentHandle.id);
            //    depthAttachmentHandle = RenderTargetHandle.CameraTarget;
            //}
        }
    }
}
