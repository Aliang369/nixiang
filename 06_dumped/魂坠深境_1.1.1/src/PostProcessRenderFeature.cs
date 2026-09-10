using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class PostProcessRenderFeature : ScriptableRendererFeature
{
	public enum RenderTargetType
	{
		Default = 0,
		EnvCameraTarget = 1,
		EnvRenderTexture = 3
	}

	[Serializable]
	public class PostProcessSettings
	{
		public RenderPassEvent renderPassEvent;

		public RenderTargetType renderSource;

		public RenderTargetType renderTarget;

		public Shader blitPS;
	}

	public PostProcessData postProcessData;

	public VolumeProfile profile;

	public PostProcessSettings settings = new PostProcessSettings();

	private PostProcessRenderPass renderPass;

	public override void Create()
	{
		renderPass = new PostProcessRenderPass(postProcessData, profile, settings);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass((ScriptableRenderPass)(object)renderPass);
	}
}
