using System;
using UnityEngine.Rendering.Universal;

public class ClearRenderFeature : ScriptableRendererFeature
{
	[Serializable]
	public class ClearPassSettings
	{
		public string passTag = "PassRenderFeature";

		public bool cleanRenderTextureColor;

		public bool cleanRenderTextureDepth;
	}

	public ClearPassSettings settings = new ClearPassSettings();

	private ClearRenderPass renderPass;

	public override void Create()
	{
		renderPass = new ClearRenderPass(settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass((ScriptableRenderPass)(object)renderPass);
	}
}
