using System;

namespace UnityEngine.Rendering.Universal;

public class FastPostProcessRenderFeature : ScriptableRendererFeature
{
	[Serializable]
	public struct FastPostProcessSettings
	{
		public bool blur = false;

		[Range(0f, 1f)]
		public float blurAmount = 1f;

		public Texture2D blurMask = null;

		public bool bloom = false;

		public Color bloomColor = Color.white;

		[Range(0f, 5f)]
		public float bloomAmount = 1f;

		[Range(0f, 1f)]
		public float bloomDiffuse = 1f;

		[Range(0f, 1f)]
		public float bloomThreshold = 0f;

		[Range(0f, 1f)]
		public float bloomSoftness = 0f;

		public bool lut = false;

		[Range(0f, 1f)]
		public float lutAmount = 0f;

		public bool imageFiltering = false;

		public Color color = Color.white;

		[Range(0f, 1f)]
		public float contrast = 0f;

		[Range(-1f, 1f)]
		public float brightness = 0f;

		[Range(-1f, 1f)]
		public float saturation = 0f;

		[Range(-1f, 1f)]
		public float exposure = 0f;

		[Range(-1f, 1f)]
		public float gamma = 0f;

		[Range(0f, 1f)]
		public float sharpness = 0f;

		public bool chromaticAberration = false;

		public float offset = 0f;

		[Range(-1f, 1f)]
		public float fishEyeDistortion = 0f;

		[Range(0f, 1f)]
		public float glitchAmount = 0f;

		public bool distortion = false;

		[Range(0f, 1f)]
		public float lensDistortion = 0f;

		public bool vignette = false;

		public Color vignetteColor = Color.black;

		[Range(0f, 1f)]
		public float vignetteAmount = 0f;

		[Range(0.001f, 1f)]
		public float vignetteSoftness = 0.001f;

		public bool HasAnyEffect
		{
			get
			{
				if (!blur && !bloom && !lut && !imageFiltering && !chromaticAberration && !distortion)
				{
					return vignette;
				}
				return true;
			}
		}

		public FastPostProcessSettings(int a = 0)
		{
		}
	}

	[Serializable]
	public class RenderFeatureSettings
	{
		public RenderPassEvent renderPassEvent;

		public RenderFeatureSettings()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			renderPassEvent = (RenderPassEvent)250;
			base._002Ector();
		}
	}

	public RenderFeatureSettings renderFeatureSettings = new RenderFeatureSettings();

	public FastPostProcessSettings settings;

	private FastPostProcessPass ppsUrpPass;

	public static FastPostProcessRenderFeature Instance { get; set; }

	public override void Create()
	{
		ppsUrpPass = new FastPostProcessPass(settings, renderFeatureSettings, ((Object)this).name);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if ((Object)(object)Instance == null)
		{
			Instance = this;
		}
		RenderingManager renderingManager = BootSetter<ApplicationStateInfo>.GetCurrentBootTarget()?.renderingManager;
		if (renderingManager != null)
		{
			renderingManager.defaultSettings = settings;
			if ((renderingManager.overrideSettings ?? settings).HasAnyEffect)
			{
				renderer.EnqueuePass((ScriptableRenderPass)(object)ppsUrpPass);
			}
		}
	}
}
