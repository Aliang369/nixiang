using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UIBlurRendererFeature : ScriptableRendererFeature
{
	[Range(1f, 4f)]
	public int downSample;

	[Range(0f, 10f)]
	public float strength;

	public RenderPassEvent renderPassEvent;

	private UIBlurRenderPass blurRenderPass;

	public override void Create()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		((Object)this).name = "UI Blur";
		blurRenderPass = new UIBlurRenderPass();
		((ScriptableRenderPass)blurRenderPass).renderPassEvent = renderPassEvent;
		blurRenderPass.downsample = downSample;
		blurRenderPass.strength = strength;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (renderingData.cameraData.cameraType == CameraType.Game && (int)renderingData.cameraData.renderType == 0 && strength > 0f)
		{
			renderer.EnqueuePass((ScriptableRenderPass)(object)blurRenderPass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		blurRenderPass.ReleaseTargets();
	}

	public UIBlurRendererFeature()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		downSample = 1;
		strength = 1f;
		renderPassEvent = (RenderPassEvent)500;
		((ScriptableRendererFeature)this)._002Ector();
	}
}
