using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PassRenderFeature : ScriptableRendererFeature
{
	[Serializable]
	public class RenderPassSettings
	{
		public string passTag;

		public RenderPassEvent renderPassEvent;

		public FilterSettings filterSettings;

		public bool cleanRenderTextureColor;

		public bool cleanRenderTextureDepth;

		public SortingCriteria criteria;

		public RenderPassSettings()
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			passTag = "PassRenderFeature";
			renderPassEvent = (RenderPassEvent)300;
			filterSettings = new FilterSettings();
			base._002Ector();
		}
	}

	[Serializable]
	public class FilterSettings
	{
		public RenderQueueType renderQueueType;

		public LayerMask layerMask;

		public string[] passNames;

		public FilterSettings()
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			base._002Ector();
			renderQueueType = (RenderQueueType)0;
			layerMask = 0;
		}
	}

	public RenderPassSettings settings = new RenderPassSettings();

	private PassRenderPass renderPass;

	public override void Create()
	{
		renderPass = new PassRenderPass(settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass((ScriptableRenderPass)(object)renderPass);
	}
}
