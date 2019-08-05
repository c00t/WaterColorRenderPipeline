using System;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;

namespace UnityEngine.Experimental.Rendering.LWRP
{
    /// <summary>
    /// Render all transparent forward objects into the given color and depth target 
    ///
    /// You can use this pass to render objects that have a material and/or shader
    /// with the pass names LightweightForward or SRPDefaultUnlit. The pass only renders
    /// objects in the rendering queue range of Transparent objects.
    /// </summary>
    internal class RenderWaterColorProxyForwardPass : ScriptableRenderPass
    {
        const string k_RenderWaterColorProxyTag = "Render Water Color Proxy(Transparents)";

        FilteringSettings m_WaterColorProxyFilterSettings;

        RenderTargetHandle colorAttachmentHandle { get; set; }
        RenderTargetHandle depthAttachmentHandle { get; set; }
        RenderTextureDescriptor descriptor { get; set; }

        RenderTargetIdentifier[] outBuffers = new RenderTargetIdentifier[7];
        PerObjectData rendererConfiguration;

        public RenderWaterColorProxyForwardPass()
        {
            RegisterShaderPassName("WaterColorProxyForward");
            //RegisterShaderPassName("SRPDefaultUnlit");

            m_WaterColorProxyFilterSettings = new FilteringSettings(new RenderQueueRange(2800,2900));
        }

        /// <summary>
        /// Configure the pass before execution
        /// </summary>
        /// <param name="baseDescriptor">Current target descriptor</param>
        /// <param name="colorAttachmentHandle">Color attachment to render into</param>
        /// <param name="depthAttachmentHandle">Depth attachment to render into</param>
        /// <param name="configuration">Specific render configuration</param>
        public void Setup(
            RenderTextureDescriptor baseDescriptor,
            RenderTargetHandle colorAttachmentHandle,
            RenderTargetHandle depthAttachmentHandle,
            RenderTargetHandle[] outHandles,
            PerObjectData configuration)
        {
            this.colorAttachmentHandle = colorAttachmentHandle;
            this.depthAttachmentHandle = depthAttachmentHandle;
            descriptor = baseDescriptor;
            rendererConfiguration = configuration;

            outBuffers[0] = colorAttachmentHandle.Identifier();
            outBuffers[1] = outHandles[2].Identifier();
            outBuffers[2] = outHandles[3].Identifier();
            outBuffers[3] = outHandles[4].Identifier();
            outBuffers[4] = outHandles[5].Identifier();
            outBuffers[5] = outHandles[6].Identifier();
            outBuffers[6] = outHandles[7].Identifier();
        }

        /// <inheritdoc/>
        public override void Execute(ScriptableRenderer renderer, ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderer == null)
                throw new ArgumentNullException("renderer");
            
            CommandBuffer cmd = CommandBufferPool.Get(k_RenderWaterColorProxyTag);
            using (new ProfilingSample(cmd, k_RenderWaterColorProxyTag))
            {
                //SetRenderTarget(cmd, colorAttachmentHandle.Identifier(), loadOp, storeOp,
                //    depthAttachmentHandle.Identifier(), loadOp, storeOp, ClearFlag.None, Color.black, descriptor.dimension);
                SetRenderTarget(cmd, outBuffers, depthAttachmentHandle.Identifier(), ClearFlag.None, Color.black);
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();

                Camera camera = renderingData.cameraData.camera;
                var drawSettings = CreateDrawingSettings(camera, SortingCriteria.CommonTransparent, rendererConfiguration, renderingData.supportsDynamicBatching, renderingData.lightData.mainLightIndex);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_WaterColorProxyFilterSettings);

                // Render objects that did not match any shader pass with error shader
                renderer.RenderObjectsWithError(context, ref renderingData.cullResults, camera, m_WaterColorProxyFilterSettings, SortingCriteria.None);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }
}
