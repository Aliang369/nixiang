using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ClearRenderPass : ScriptableRenderPass
{
	private readonly ClearRenderFeature.ClearPassSettings settings;

	public ClearRenderPass(ClearRenderFeature.ClearPassSettings settings)
	{
		this.settings = settings;
		((ScriptableRenderPass)this).renderPassEvent = (RenderPassEvent)150;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType != CameraType.SceneView)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get(settings.passTag);
			commandBuffer.ClearRenderTarget(settings.cleanRenderTextureDepth, settings.cleanRenderTextureColor, Color.black);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
