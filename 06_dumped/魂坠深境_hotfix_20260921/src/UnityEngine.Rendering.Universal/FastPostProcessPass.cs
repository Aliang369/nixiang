namespace UnityEngine.Rendering.Universal;

public class FastPostProcessPass : ScriptableRenderPass
{
	private static readonly int blurTexString = Shader.PropertyToID("_BlurTex");

	private static readonly int maskTextureString = Shader.PropertyToID("_MaskTex");

	private static readonly int blurAmountString = Shader.PropertyToID("_BlurAmount");

	private static readonly int bloomColorString = Shader.PropertyToID("_BloomColor");

	private static readonly int blDiffuseString = Shader.PropertyToID("_BloomDiffuse");

	private static readonly int blDataString = Shader.PropertyToID("_BloomData");

	private static readonly int lutTextureString = Shader.PropertyToID("_LutTex");

	private static readonly int lutAmountString = Shader.PropertyToID("_LutAmount");

	private static readonly int colorString = Shader.PropertyToID("_Color");

	private static readonly int contrastString = Shader.PropertyToID("_Contrast");

	private static readonly int brightnessString = Shader.PropertyToID("_Brightness");

	private static readonly int saturationString = Shader.PropertyToID("_Saturation");

	private static readonly int centralFactorString = Shader.PropertyToID("_CentralFactor");

	private static readonly int sideFactorString = Shader.PropertyToID("_SideFactor");

	private static readonly int offsetString = Shader.PropertyToID("_Offset");

	private static readonly int fishEyeString = Shader.PropertyToID("_FishEye");

	private static readonly int lensdistortionString = Shader.PropertyToID("_LensDistortion");

	private static readonly int vignetteColorString = Shader.PropertyToID("_VignetteColor");

	private static readonly int vignetteAmountString = Shader.PropertyToID("_VignetteAmount");

	private static readonly int vignetteSoftnessString = Shader.PropertyToID("_VignetteSoftness");

	private static readonly int blurTempString = Shader.PropertyToID("_BlurTemp");

	private static readonly int blurTemp1String = Shader.PropertyToID("_BlurTemp1");

	private static readonly int tempCopyString = Shader.PropertyToID("_TempCopy");

	private static readonly string bloomKeyword = "BLOOM";

	private static readonly string blurKeyword = "BLUR";

	private static readonly string chromaKeyword = "CHROMA";

	private static readonly string lutKeyword = "LUT";

	private static readonly string filterKeyword = "FILTER";

	private static readonly string shaprenKeyword = "SHARPEN";

	private static readonly string distortionKeyword = "DISTORTION";

	private readonly RenderTargetIdentifier blurTemp;

	private readonly RenderTargetIdentifier blurTemp1;

	private readonly RenderTargetIdentifier blurTex;

	private Texture3D converted3D;

	private readonly FastPostProcessRenderFeature.FastPostProcessSettings defaultSettings;

	private bool maskSet;

	private readonly string name;

	private int numberOfPasses;

	private Texture2D previous;

	private FastPostProcessRenderFeature.RenderFeatureSettings renderFeatureSettings;

	private RenderTargetIdentifier source;

	private Texture2D sourceLut;

	private float t;

	private float a;

	private float knee;

	private readonly RenderTargetIdentifier tempCopy;

	private Material fastProcessMat;

	public FilterMode filterMode { get; set; }

	public FastPostProcessPass(FastPostProcessRenderFeature.FastPostProcessSettings defaultSettings, FastPostProcessRenderFeature.RenderFeatureSettings renderFeatureSettings, string name)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		blurTemp = new RenderTargetIdentifier(blurTempString);
		blurTemp1 = new RenderTargetIdentifier(blurTemp1String);
		blurTex = new RenderTargetIdentifier(blurTexString);
		numberOfPasses = 3;
		tempCopy = new RenderTargetIdentifier(tempCopyString);
		((ScriptableRenderPass)this)._002Ector();
		this.defaultSettings = defaultSettings;
		this.name = name;
		this.renderFeatureSettings = renderFeatureSettings;
		((ScriptableRenderPass)this).renderPassEvent = renderFeatureSettings.renderPassEvent;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		ApplicationStateInfo currentBootTarget = BootSetter<ApplicationStateInfo>.GetCurrentBootTarget();
		if (currentBootTarget?.renderingManager == null)
		{
			return;
		}
		currentBootTarget.renderingManager.defaultSettings = defaultSettings;
		source = CameraExtensions.GetUniversalAdditionalCameraData(renderingData.cameraData.camera).scriptableRenderer.cameraColorTarget;
		int pixelWidth = renderingData.cameraData.camera.pixelWidth;
		int pixelHeight = renderingData.cameraData.camera.pixelHeight;
		if ((object)fastProcessMat == null)
		{
			fastProcessMat = new Material(ScriptableObjectSingleton<GameDataManager>.instance.fastPostProcessMat);
		}
		FastPostProcessRenderFeature.FastPostProcessSettings settings = currentBootTarget.renderingManager.overrideSettings ?? defaultSettings;
		CommandBuffer commandBuffer = CommandBufferPool.Get(name);
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
		cameraTargetDescriptor.depthBufferBits = 0;
		commandBuffer.GetTemporaryRT(tempCopyString, cameraTargetDescriptor, FilterMode.Bilinear);
		commandBuffer.Blit(source, tempCopy);
		if (settings.bloom || settings.blur)
		{
			fastProcessMat.DisableKeyword(blurKeyword);
			fastProcessMat.DisableKeyword(bloomKeyword);
			if (settings.bloom)
			{
				fastProcessMat.EnableKeyword(bloomKeyword);
				fastProcessMat.SetColor(bloomColorString, settings.bloomColor * settings.bloomAmount);
				fastProcessMat.SetFloat(blDiffuseString, settings.bloomDiffuse);
				numberOfPasses = Mathf.Max(Mathf.CeilToInt(settings.bloomDiffuse * 4f), 1);
				fastProcessMat.SetFloat(blDiffuseString, (numberOfPasses > 1) ? ((settings.bloomDiffuse * 4f - (float)Mathf.FloorToInt(settings.bloomDiffuse * 4f - 0.001f)) * 0.5f + 0.5f) : (settings.bloomDiffuse * 4f));
				knee = settings.bloomThreshold * settings.bloomSoftness;
				fastProcessMat.SetVector(blDataString, new Vector4(settings.bloomThreshold, settings.bloomThreshold - knee, 2f * knee, 1f / (4f * knee + 1E-05f)));
			}
			if (settings.blur)
			{
				fastProcessMat.EnableKeyword(blurKeyword);
				numberOfPasses = Mathf.Max(Mathf.CeilToInt(settings.blurAmount * 4f), 1);
				fastProcessMat.SetFloat(blurAmountString, (numberOfPasses > 1) ? ((settings.blurAmount * 4f - (float)Mathf.FloorToInt(settings.blurAmount * 4f - 0.001f)) * 0.5f + 0.5f) : (settings.blurAmount * 4f));
				fastProcessMat.SetTexture(maskTextureString, settings.blurMask);
			}
			if (settings.blurAmount > 0f || !settings.blur)
			{
				if (numberOfPasses == 1)
				{
					commandBuffer.GetTemporaryRT(blurTexString, pixelWidth / 2, pixelHeight / 2, 0, FilterMode.Bilinear);
					commandBuffer.Blit(tempCopy, blurTex, fastProcessMat, 0);
				}
				else if (numberOfPasses == 2)
				{
					commandBuffer.GetTemporaryRT(blurTexString, pixelWidth / 2, pixelHeight / 2, 0, FilterMode.Bilinear);
					commandBuffer.GetTemporaryRT(blurTempString, pixelWidth / 4, pixelHeight / 4, 0, FilterMode.Bilinear);
					commandBuffer.Blit(tempCopy, blurTemp, fastProcessMat, 0);
					commandBuffer.Blit(blurTemp, blurTex, fastProcessMat, 0);
				}
				else if (numberOfPasses == 3)
				{
					commandBuffer.GetTemporaryRT(blurTexString, pixelWidth / 4, pixelHeight / 4, 0, FilterMode.Bilinear);
					commandBuffer.GetTemporaryRT(blurTempString, pixelWidth / 8, pixelHeight / 8, 0, FilterMode.Bilinear);
					commandBuffer.Blit(tempCopy, blurTex, fastProcessMat, 0);
					commandBuffer.Blit(blurTex, blurTemp, fastProcessMat, 0);
					commandBuffer.Blit(blurTemp, blurTex, fastProcessMat, 0);
				}
				else if (numberOfPasses == 4)
				{
					commandBuffer.GetTemporaryRT(blurTexString, pixelWidth / 4, pixelHeight / 4, 0, FilterMode.Bilinear);
					commandBuffer.GetTemporaryRT(blurTempString, pixelWidth / 8, pixelHeight / 8, 0, FilterMode.Bilinear);
					commandBuffer.GetTemporaryRT(blurTemp1String, pixelWidth / 16, pixelHeight / 16, 0, FilterMode.Bilinear);
					commandBuffer.Blit(tempCopy, blurTex, fastProcessMat, 0);
					commandBuffer.Blit(blurTex, blurTemp, fastProcessMat, 0);
					commandBuffer.Blit(blurTemp, blurTemp1, fastProcessMat, 0);
					commandBuffer.Blit(blurTemp1, blurTemp, fastProcessMat, 0);
					commandBuffer.Blit(blurTemp, blurTex, fastProcessMat, 0);
				}
				commandBuffer.SetGlobalTexture(blurTexString, blurTex);
			}
			else
			{
				commandBuffer.SetGlobalTexture(blurTexString, tempCopy);
			}
		}
		else
		{
			fastProcessMat.DisableKeyword(blurKeyword);
			fastProcessMat.DisableKeyword(bloomKeyword);
		}
		if (settings.lut)
		{
			isConverted(settings);
			fastProcessMat.EnableKeyword(lutKeyword);
			fastProcessMat.SetFloat(lutAmountString, settings.lutAmount);
			fastProcessMat.SetTexture(lutTextureString, converted3D);
		}
		else
		{
			fastProcessMat.DisableKeyword(lutKeyword);
		}
		if (settings.imageFiltering)
		{
			fastProcessMat.EnableKeyword(filterKeyword);
			fastProcessMat.SetColor(colorString, (Mathf.Pow(2f, settings.exposure) - settings.gamma) * settings.color);
			fastProcessMat.SetFloat(contrastString, settings.contrast + 1f);
			fastProcessMat.SetFloat(brightnessString, settings.brightness * 0.5f - settings.contrast);
			fastProcessMat.SetFloat(saturationString, settings.saturation + 1f);
			if (settings.sharpness > 0f)
			{
				fastProcessMat.EnableKeyword(shaprenKeyword);
				fastProcessMat.SetFloat(centralFactorString, 1f + 3.2f * settings.sharpness);
				fastProcessMat.SetFloat(sideFactorString, 0.8f * settings.sharpness);
			}
			else
			{
				fastProcessMat.DisableKeyword(shaprenKeyword);
			}
		}
		else
		{
			fastProcessMat.DisableKeyword(filterKeyword);
			fastProcessMat.DisableKeyword(shaprenKeyword);
		}
		if (settings.chromaticAberration)
		{
			fastProcessMat.EnableKeyword(chromaKeyword);
			if (settings.glitchAmount > 0f)
			{
				t = Time.realtimeSinceStartup;
				a = (1f + Mathf.Sin(t * 6f)) * (0.5f + Mathf.Sin(t * 16f) * 0.25f) * (0.5f + Mathf.Sin(t * 19f) * 0.25f) * (0.5f + Mathf.Sin(t * 27f) * 0.25f);
				fastProcessMat.SetFloat(offsetString, 10f * settings.offset + settings.glitchAmount * Mathf.Pow(a, 3f) * 200f);
			}
			else
			{
				fastProcessMat.SetFloat(offsetString, 10f * settings.offset);
			}
			fastProcessMat.SetFloat(fishEyeString, 0.1f * settings.fishEyeDistortion);
		}
		else
		{
			fastProcessMat.DisableKeyword(chromaKeyword);
		}
		if (settings.distortion)
		{
			fastProcessMat.SetFloat(lensdistortionString, 0f - settings.lensDistortion);
			fastProcessMat.EnableKeyword(distortionKeyword);
		}
		else
		{
			fastProcessMat.DisableKeyword(distortionKeyword);
		}
		if (settings.vignette)
		{
			fastProcessMat.SetColor(vignetteColorString, settings.vignetteColor);
			fastProcessMat.SetFloat(vignetteAmountString, 1f - settings.vignetteAmount);
			fastProcessMat.SetFloat(vignetteSoftnessString, 1f - settings.vignetteSoftness - settings.vignetteAmount);
		}
		else
		{
			fastProcessMat.SetFloat(vignetteAmountString, 1f);
			fastProcessMat.SetFloat(vignetteSoftnessString, 0.999f);
		}
		commandBuffer.Blit(tempCopy, source, fastProcessMat, 1);
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(tempCopyString);
		cmd.ReleaseTemporaryRT(blurTempString);
		cmd.ReleaseTemporaryRT(blurTemp1String);
		cmd.ReleaseTemporaryRT(blurTexString);
	}

	private void isConverted(FastPostProcessRenderFeature.FastPostProcessSettings settings)
	{
		if (sourceLut != previous)
		{
			previous = sourceLut;
			Convert(sourceLut);
		}
	}

	private void Convert(Texture2D tempTex)
	{
		Color[] pixels = tempTex.GetPixels();
		Color[] array = new Color[pixels.Length];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				for (int k = 0; k < 16; k++)
				{
					int num = 16 - j - 1;
					array[i + j * 16 + k * 256] = pixels[k * 16 + i + num * 256];
				}
			}
		}
		if ((bool)converted3D)
		{
			Object.DestroyImmediate(converted3D);
		}
		converted3D = new Texture3D(16, 16, 16, TextureFormat.ARGB32, mipChain: false)
		{
			hideFlags = HideFlags.HideAndDontSave,
			wrapMode = TextureWrapMode.Clamp
		};
		converted3D.SetPixels(array);
		converted3D.Apply();
	}
}
