using System;
using System.Runtime.CompilerServices;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.Universal.Internal;

public class PostProcessRenderPass : ScriptableRenderPass
{
	private class MaterialLibrary
	{
		public readonly Material bloom;

		public readonly Material bokehDepthOfField;

		public readonly Material cameraMotionBlur;

		public readonly Material finalPass;

		public readonly Material gaussianDepthOfField;

		public readonly Material paniniProjection;

		public readonly Material stopNaN;

		public readonly Material subpixelMorphologicalAntialiasing;

		public readonly Material uber;

		public MaterialLibrary(PostProcessData data)
		{
			stopNaN = Load(data.shaders.stopNanPS);
			subpixelMorphologicalAntialiasing = Load(data.shaders.subpixelMorphologicalAntialiasingPS);
			gaussianDepthOfField = Load(data.shaders.gaussianDepthOfFieldPS);
			bokehDepthOfField = Load(data.shaders.bokehDepthOfFieldPS);
			cameraMotionBlur = Load(data.shaders.cameraMotionBlurPS);
			paniniProjection = Load(data.shaders.paniniProjectionPS);
			bloom = Load(data.shaders.bloomPS);
			uber = Load(data.shaders.uberPostPS);
			finalPass = Load(data.shaders.finalPostPassPS);
		}

		private Material Load(Shader shader)
		{
			if (shader == null)
			{
				Debug.LogErrorFormat("Missing shader. " + GetType().DeclaringType.Name + " render pass will not execute. Check for missing reference in the renderer resources.");
				return null;
			}
			if (!shader.isSupported)
			{
				return null;
			}
			return CoreUtils.CreateEngineMaterial(shader);
		}

		internal void Cleanup()
		{
			CoreUtils.Destroy(stopNaN);
			CoreUtils.Destroy(subpixelMorphologicalAntialiasing);
			CoreUtils.Destroy(gaussianDepthOfField);
			CoreUtils.Destroy(bokehDepthOfField);
			CoreUtils.Destroy(cameraMotionBlur);
			CoreUtils.Destroy(paniniProjection);
			CoreUtils.Destroy(bloom);
			CoreUtils.Destroy(uber);
			CoreUtils.Destroy(finalPass);
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _TempTarget = Shader.PropertyToID("_TempTarget");

		public static readonly int _TempTarget2 = Shader.PropertyToID("_TempTarget2");

		public static readonly int _StencilRef = Shader.PropertyToID("_StencilRef");

		public static readonly int _StencilMask = Shader.PropertyToID("_StencilMask");

		public static readonly int _FullCoCTexture = Shader.PropertyToID("_FullCoCTexture");

		public static readonly int _HalfCoCTexture = Shader.PropertyToID("_HalfCoCTexture");

		public static readonly int _DofTexture = Shader.PropertyToID("_DofTexture");

		public static readonly int _CoCParams = Shader.PropertyToID("_CoCParams");

		public static readonly int _BokehKernel = Shader.PropertyToID("_BokehKernel");

		public static readonly int _PongTexture = Shader.PropertyToID("_PongTexture");

		public static readonly int _PingTexture = Shader.PropertyToID("_PingTexture");

		public static readonly int _Metrics = Shader.PropertyToID("_Metrics");

		public static readonly int _AreaTexture = Shader.PropertyToID("_AreaTexture");

		public static readonly int _SearchTexture = Shader.PropertyToID("_SearchTexture");

		public static readonly int _EdgeTexture = Shader.PropertyToID("_EdgeTexture");

		public static readonly int _BlendTexture = Shader.PropertyToID("_BlendTexture");

		public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");

		public static readonly int _Params = Shader.PropertyToID("_Params");

		public static readonly int _MainTexLowMip = Shader.PropertyToID("_MainTexLowMip");

		public static readonly int _Bloom_Params = Shader.PropertyToID("_Bloom_Params");

		public static readonly int _Bloom_RGBM = Shader.PropertyToID("_Bloom_RGBM");

		public static readonly int _Bloom_Texture = Shader.PropertyToID("_Bloom_Texture");

		public static readonly int _LensDirt_Texture = Shader.PropertyToID("_LensDirt_Texture");

		public static readonly int _LensDirt_Params = Shader.PropertyToID("_LensDirt_Params");

		public static readonly int _LensDirt_Intensity = Shader.PropertyToID("_LensDirt_Intensity");

		public static readonly int _Distortion_Params1 = Shader.PropertyToID("_Distortion_Params1");

		public static readonly int _Distortion_Params2 = Shader.PropertyToID("_Distortion_Params2");

		public static readonly int _Chroma_Params = Shader.PropertyToID("_Chroma_Params");

		public static readonly int _Vignette_Params1 = Shader.PropertyToID("_Vignette_Params1");

		public static readonly int _Vignette_Params2 = Shader.PropertyToID("_Vignette_Params2");

		public static readonly int _Lut_Params = Shader.PropertyToID("_Lut_Params");

		public static readonly int _UserLut_Params = Shader.PropertyToID("_UserLut_Params");

		public static readonly int _InternalLut = Shader.PropertyToID("_InternalLut");

		public static readonly int _UserLut = Shader.PropertyToID("_UserLut");

		public static readonly int _FullscreenProjMat = Shader.PropertyToID("_FullscreenProjMat");

		public static int[] _BloomMipUp;

		public static int[] _BloomMipDown;
	}

	private const string k_RenderPostProcessingTag = "Render PostProcessing Effects";

	private const string k_RenderFinalPostProcessingTag = "Render Final PostProcessing Pass";

	private const int k_MaxPyramidSize = 16;

	private static readonly int m_InternalLutID = Shader.PropertyToID("_InternalGradingLut");

	private readonly GraphicsFormat m_DefaultHDRFormat;

	private readonly GraphicsFormat m_GaussianCoCFormat;

	private readonly GraphicsFormat m_SMAAEdgeFormat;

	private ApplicationStateInfo applicationState;

	private int bufferHeight;

	private Rect bufferRect;

	private int bufferWidth;

	private RenderTargetIdentifier destinationColor;

	private RenderTargetIdentifier destinationDepth;

	private readonly Material m_BlitMaterial;

	private Bloom m_Bloom;

	private int m_BokehHash;

	private Vector4[] m_BokehKernel;

	private ChromaticAberration m_ChromaticAberration;

	private ColorAdjustments m_ColorAdjustments;

	private ColorLookup m_ColorLookup;

	private readonly PostProcessData m_Data;

	private DepthOfField m_DepthOfField;

	private RenderTextureDescriptor m_Descriptor;

	private int m_DitheringTextureIndex;

	private bool m_EnableSRGBConversionIfNeeded;

	private FilmGrain m_FilmGrain;

	private bool m_HasFinalPass;

	private bool m_IsFinalPass;

	private bool m_IsStereo;

	private LensDistortion m_LensDistortion;

	private readonly MaterialLibrary m_Materials;

	private MotionBlur m_MotionBlur;

	private readonly RenderTargetIdentifier[] m_MRT2;

	private PaniniProjection m_PaniniProjection;

	private Matrix4x4 m_PrevViewProjM;

	private bool m_ResetHistory;

	private Tonemapping m_Tonemapping;

	private readonly bool m_UseRGBM;

	private Vignette m_Vignette;

	private readonly VolumeProfile profile;

	private PostProcessRenderFeature.PostProcessSettings settings;

	private RenderTargetIdentifier sourceColor;

	private RenderTargetIdentifier sourceDepth;

	public PostProcessRenderPass(PostProcessData postProcessData, VolumeProfile profile, PostProcessRenderFeature.PostProcessSettings settings)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		m_PrevViewProjM = Matrix4x4.identity;
		((ScriptableRenderPass)this)._002Ector();
		this.settings = settings;
		this.profile = profile;
		((ScriptableRenderPass)this).renderPassEvent = settings.renderPassEvent;
		m_Data = postProcessData;
		m_Materials = new MaterialLibrary(postProcessData);
		m_BlitMaterial = CoreUtils.CreateEngineMaterial(settings.blitPS);
		if (SystemInfo.IsFormatSupported(GraphicsFormat.B10G11R11_UFloatPack32, FormatUsage.Blend))
		{
			m_DefaultHDRFormat = GraphicsFormat.B10G11R11_UFloatPack32;
			m_UseRGBM = false;
		}
		else
		{
			m_DefaultHDRFormat = ((QualitySettings.activeColorSpace == ColorSpace.Linear) ? GraphicsFormat.R8G8B8A8_SRGB : GraphicsFormat.R8G8B8A8_UNorm);
			m_UseRGBM = true;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R8G8_UNorm, FormatUsage.Render) && SystemInfo.graphicsDeviceVendor.ToLowerInvariant().Contains("arm"))
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8_UNorm;
		}
		else
		{
			m_SMAAEdgeFormat = GraphicsFormat.R8G8B8A8_UNorm;
		}
		if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_UNorm, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_UNorm;
		}
		else if (SystemInfo.IsFormatSupported(GraphicsFormat.R16_SFloat, FormatUsage.Blend))
		{
			m_GaussianCoCFormat = GraphicsFormat.R16_SFloat;
		}
		else
		{
			m_GaussianCoCFormat = GraphicsFormat.R8_UNorm;
		}
		ShaderConstants._BloomMipUp = new int[16];
		ShaderConstants._BloomMipDown = new int[16];
		for (int i = 0; i < 16; i++)
		{
			ShaderConstants._BloomMipUp[i] = Shader.PropertyToID("_BloomMipUp" + i);
			ShaderConstants._BloomMipDown[i] = Shader.PropertyToID("_BloomMipDown" + i);
		}
		m_MRT2 = new RenderTargetIdentifier[2];
		m_ResetHistory = true;
	}

	public void Cleanup()
	{
		m_Materials.Cleanup();
	}

	public void SetupPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		sourceColor = renderingData.cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraColorTarget;
		sourceDepth = renderingData.cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraDepthTarget;
		bufferHeight = renderingData.cameraData.camera.pixelHeight;
		bufferWidth = renderingData.cameraData.camera.pixelWidth;
		m_Descriptor = renderingData.cameraData.cameraTargetDescriptor;
		m_Descriptor = new RenderTextureDescriptor(bufferWidth, bufferHeight, renderingData.cameraData.cameraTargetDescriptor.colorFormat, renderingData.cameraData.cameraTargetDescriptor.depthBufferBits);
		sourceColor = renderingData.cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraColorTarget;
		sourceDepth = renderingData.cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraDepthTarget;
		bufferRect = new Rect(0f, 0f, bufferWidth, bufferHeight);
		m_IsFinalPass = false;
		m_HasFinalPass = true;
		m_EnableSRGBConversionIfNeeded = true;
	}

	public void SetupFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RenderTargetHandle cameraTarget = RenderTargetHandle.CameraTarget;
		destinationColor = ((RenderTargetHandle)(ref cameraTarget)).Identifier();
		m_IsFinalPass = true;
		m_HasFinalPass = false;
		m_EnableSRGBConversionIfNeeded = true;
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		RenderTargetIdentifier renderTargetIdentifier = destinationColor;
		RenderTargetHandle cameraTarget = RenderTargetHandle.CameraTarget;
		if (!(renderTargetIdentifier == ((RenderTargetHandle)(ref cameraTarget)).Identifier()))
		{
			RenderTextureDescriptor desc = cameraTextureDescriptor;
			desc.msaaSamples = 1;
			desc.depthBufferBits = 0;
			cmd.GetTemporaryRT(0, desc, FilterMode.Point);
		}
	}

	public void ResetHistory()
	{
		m_ResetHistory = true;
	}

	public bool CanRunOnTile()
	{
		return false;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Expected O, but got Unknown
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Expected O, but got Unknown
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Expected O, but got Unknown
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Expected O, but got Unknown
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Expected O, but got Unknown
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Expected O, but got Unknown
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Expected O, but got Unknown
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Expected O, but got Unknown
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Expected O, but got Unknown
		foreach (VolumeComponent component in profile.components)
		{
			if (component is DepthOfField)
			{
				if (!component.active)
				{
					m_DepthOfField = new DepthOfField();
				}
				else
				{
					m_DepthOfField = (DepthOfField)(object)((component is DepthOfField) ? component : null);
				}
			}
			else if (component is MotionBlur)
			{
				if (!component.active)
				{
					m_MotionBlur = new MotionBlur();
				}
				else
				{
					m_MotionBlur = (MotionBlur)(object)((component is MotionBlur) ? component : null);
				}
			}
			else if (component is PaniniProjection)
			{
				if (!component.active)
				{
					m_PaniniProjection = new PaniniProjection();
				}
				else
				{
					m_PaniniProjection = (PaniniProjection)(object)((component is PaniniProjection) ? component : null);
				}
			}
			else if (component is Bloom)
			{
				if (!component.active)
				{
					m_Bloom = new Bloom();
				}
				else
				{
					m_Bloom = (Bloom)(object)((component is Bloom) ? component : null);
				}
			}
			else if (component is LensDistortion)
			{
				if (!component.active)
				{
					m_LensDistortion = new LensDistortion();
				}
				else
				{
					m_LensDistortion = (LensDistortion)(object)((component is LensDistortion) ? component : null);
				}
			}
			else if (component is ChromaticAberration)
			{
				if (!component.active)
				{
					m_ChromaticAberration = new ChromaticAberration();
				}
				else
				{
					m_ChromaticAberration = (ChromaticAberration)(object)((component is ChromaticAberration) ? component : null);
				}
			}
			else if (component is Vignette)
			{
				if (!component.active)
				{
					m_Vignette = new Vignette();
				}
				else
				{
					m_Vignette = (Vignette)(object)((component is Vignette) ? component : null);
				}
			}
			else if (component is ColorLookup)
			{
				if (!component.active)
				{
					m_ColorLookup = new ColorLookup();
				}
				else
				{
					m_ColorLookup = (ColorLookup)(object)((component is ColorLookup) ? component : null);
				}
			}
			else if (component is ColorAdjustments)
			{
				if (!component.active)
				{
					m_ColorAdjustments = new ColorAdjustments();
				}
				else
				{
					m_ColorAdjustments = (ColorAdjustments)(object)((component is ColorAdjustments) ? component : null);
				}
			}
			else if (component is Tonemapping)
			{
				if (!component.active)
				{
					m_Tonemapping = new Tonemapping();
				}
				else
				{
					m_Tonemapping = (Tonemapping)(object)((component is Tonemapping) ? component : null);
				}
			}
			else if (component is FilmGrain)
			{
				if (!component.active)
				{
					m_FilmGrain = new FilmGrain();
				}
				else
				{
					m_FilmGrain = (FilmGrain)(object)((component is FilmGrain) ? component : null);
				}
			}
		}
		if (!m_IsFinalPass && !CanRunOnTile())
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("Render PostProcessing Effects");
			Render(commandBuffer, ref renderingData);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
		m_ResetHistory = false;
	}

	private RenderTextureDescriptor GetStereoCompatibleDescriptor()
	{
		return GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_Descriptor.graphicsFormat, m_Descriptor.depthBufferBits);
	}

	private RenderTextureDescriptor GetStereoCompatibleDescriptor(int width, int height, GraphicsFormat format, int depthBufferBits = 0)
	{
		RenderTextureDescriptor descriptor = m_Descriptor;
		descriptor.depthBufferBits = depthBufferBits;
		descriptor.msaaSamples = 1;
		descriptor.width = width;
		descriptor.height = height;
		descriptor.graphicsFormat = format;
		return descriptor;
	}

	private bool RequireSRGBConversionBlitToBackBuffer(CameraData cameraData)
	{
		return Display.main.requiresSrgbBlitToBackbuffer;
	}

	private void Render(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Invalid comparison between Unknown and I4
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Invalid comparison between Unknown and I4
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03db: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		SetupPass(cmd, ref renderingData);
		ref CameraData cameraData = ref renderingData.cameraData;
		m_IsStereo = ((CameraData)(ref renderingData.cameraData)).xr.enabled;
		bool tempTargetUsed = false;
		bool tempTarget2Used = false;
		RenderTargetIdentifier tempDestination = -1;
		bool isSceneViewCamera = ((CameraData)(ref cameraData)).isSceneViewCamera;
		m_IsFinalPass = false;
		cmd.SetGlobalMatrix(ShaderConstants._FullscreenProjMat, GL.GetGPUProjectionMatrix(Matrix4x4.identity, renderIntoTexture: true));
		if (cameraData.isStopNaNEnabled && m_Materials.stopNaN != null)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.StopNaNs)))
			{
				cmd.Blit(sourceColor, BlitDstDiscardContent(cmd, GetDestination()), m_Materials.stopNaN);
				Swap();
			}
		}
		if ((int)cameraData.antialiasing == 2 && SystemInfo.graphicsDeviceType != GraphicsDeviceType.OpenGLES2)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.SMAA)))
			{
				DoSubpixelMorphologicalAntialiasing(ref cameraData, cmd, sourceColor, GetDestination());
				Swap();
			}
		}
		if (m_DepthOfField.IsActive() && !isSceneViewCamera)
		{
			URPProfileId marker = (((int)((VolumeParameter<DepthOfFieldMode>)(object)m_DepthOfField.mode).value == 1) ? URPProfileId.GaussianDepthOfField : URPProfileId.BokehDepthOfField);
			using (new ProfilingScope(cmd, ProfilingSampler.Get(marker)))
			{
				DoDepthOfField(cameraData.camera, cmd, sourceColor, GetDestination(), bufferRect);
				Swap();
			}
		}
		if (m_MotionBlur.IsActive() && !isSceneViewCamera)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.MotionBlur)))
			{
				DoMotionBlur(cameraData.camera, cmd, sourceColor, GetDestination());
				Swap();
			}
		}
		if (m_PaniniProjection.IsActive() && !isSceneViewCamera)
		{
			using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.PaniniProjection)))
			{
				DoPaniniProjection(cameraData.camera, cmd, sourceColor, GetDestination());
				Swap();
			}
		}
		using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.UberPostProcess)))
		{
			m_Materials.uber.shaderKeywords = null;
			bool flag = m_Bloom.IsActive();
			if (flag)
			{
				using (new ProfilingScope(cmd, ProfilingSampler.Get(URPProfileId.Bloom)))
				{
					SetupBloom(cmd, sourceColor, m_Materials.uber);
				}
			}
			SetupLensDistortion(m_Materials.uber, isSceneViewCamera);
			SetupChromaticAberration(m_Materials.uber);
			SetupVignette(m_Materials.uber);
			SetupColorGrading(cmd, ref renderingData, m_Materials.uber);
			SetupGrain(in cameraData, m_Materials.uber);
			SetupDithering(in cameraData, m_Materials.uber);
			if (RequireSRGBConversionBlitToBackBuffer(cameraData) && m_EnableSRGBConversionIfNeeded)
			{
				m_Materials.uber.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
			}
			if (!tempTargetUsed)
			{
				cmd.Blit(sourceColor, GetDestination());
				Swap();
			}
			cmd.SetGlobalTexture("_BlitTex", sourceColor);
			RenderBufferLoadAction colorLoadAction = RenderBufferLoadAction.DontCare;
			RenderTargetIdentifier renderTargetIdentifier = tempDestination;
			RenderTargetHandle cameraTarget = RenderTargetHandle.CameraTarget;
			if (renderTargetIdentifier == ((RenderTargetHandle)(ref cameraTarget)).Identifier() && !cameraData.isDefaultViewport)
			{
				colorLoadAction = RenderBufferLoadAction.Load;
			}
			cmd.SetRenderTarget(destinationColor, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
			int num;
			if (!cameraData.resolveFinalTarget)
			{
				RenderTargetIdentifier renderTargetIdentifier2 = destinationColor;
				cameraTarget = RenderTargetHandle.CameraTarget;
				if (!(renderTargetIdentifier2 == ((RenderTargetHandle)(ref cameraTarget)).Identifier()))
				{
					num = (m_HasFinalPass ? 1 : 0);
					goto IL_043d;
				}
			}
			num = 1;
			goto IL_043d;
			IL_043d:
			bool flag2 = (byte)num != 0;
			if (m_IsStereo)
			{
				((ScriptableRenderPass)this).Blit(cmd, sourceColor, (RenderTargetIdentifier)BuiltinRenderTextureType.CurrentActive, m_Materials.uber, 0);
				if (!flag2)
				{
					cmd.SetGlobalTexture("_BlitTex", destinationColor);
					((ScriptableRenderPass)this).Blit(cmd, (RenderTargetIdentifier)BuiltinRenderTextureType.CurrentActive, sourceColor, m_BlitMaterial, 0);
				}
			}
			else
			{
				cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
				RenderTargetIdentifier renderTargetIdentifier3 = tempDestination;
				cameraTarget = RenderTargetHandle.CameraTarget;
				if (renderTargetIdentifier3 == ((RenderTargetHandle)(ref cameraTarget)).Identifier())
				{
					cmd.SetViewport(bufferRect);
				}
				cmd.SetGlobalTexture("_BlitTex", sourceColor);
				cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_Materials.uber);
				if (!flag2)
				{
					cmd.SetGlobalTexture("_BlitTex", destinationColor);
					cmd.SetRenderTarget(sourceColor, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
					cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
				}
				cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
			}
			if (flag)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[0]);
			}
			if (tempTargetUsed)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget);
			}
			if (tempTarget2Used)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._TempTarget2);
			}
			cmd.SetRenderTarget(cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraColorTarget, cameraData.camera.GetComponent<UniversalAdditionalCameraData>().scriptableRenderer.cameraDepthTarget);
		}
		RenderTargetIdentifier GetDestination()
		{
			if (tempDestination == -1)
			{
				cmd.GetTemporaryRT(ShaderConstants._TempTarget, GetStereoCompatibleDescriptor(), FilterMode.Bilinear);
				tempDestination = ShaderConstants._TempTarget;
				tempTargetUsed = true;
			}
			else if (tempDestination == sourceColor && m_Descriptor.msaaSamples > 1)
			{
				cmd.GetTemporaryRT(ShaderConstants._TempTarget2, GetStereoCompatibleDescriptor(), FilterMode.Bilinear);
				tempDestination = ShaderConstants._TempTarget2;
				tempTarget2Used = true;
			}
			return tempDestination;
		}
		void Swap()
		{
			CoreUtils.Swap(ref sourceColor, ref tempDestination);
		}
	}

	private BuiltinRenderTextureType BlitDstDiscardContent(CommandBuffer cmd, RenderTargetIdentifier rt)
	{
		cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
		return BuiltinRenderTextureType.CurrentActive;
	}

	private void DoSubpixelMorphologicalAntialiasing(ref CameraData cameraData, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Expected I4, but got Unknown
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = cameraData.camera;
		_ = bufferRect;
		Material subpixelMorphologicalAntialiasing = m_Materials.subpixelMorphologicalAntialiasing;
		subpixelMorphologicalAntialiasing.SetVector(ShaderConstants._Metrics, new Vector4(1f / (float)m_Descriptor.width, 1f / (float)m_Descriptor.height, m_Descriptor.width, m_Descriptor.height));
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._AreaTexture, m_Data.textures.smaaAreaTex);
		subpixelMorphologicalAntialiasing.SetTexture(ShaderConstants._SearchTexture, m_Data.textures.smaaSearchTex);
		subpixelMorphologicalAntialiasing.SetInt(ShaderConstants._StencilRef, 64);
		subpixelMorphologicalAntialiasing.SetInt(ShaderConstants._StencilMask, 64);
		subpixelMorphologicalAntialiasing.shaderKeywords = null;
		AntialiasingQuality antialiasingQuality = cameraData.antialiasingQuality;
		switch ((int)antialiasingQuality)
		{
		case 0:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_LOW");
			break;
		case 1:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_MEDIUM");
			break;
		case 2:
			subpixelMorphologicalAntialiasing.EnableKeyword("_SMAA_PRESET_HIGH");
			break;
		}
		RenderTargetIdentifier renderTargetIdentifier = sourceDepth;
		RenderTargetHandle cameraTarget = RenderTargetHandle.CameraTarget;
		RenderTargetIdentifier depth;
		int depthBufferBits;
		if (renderTargetIdentifier == ((RenderTargetHandle)(ref cameraTarget)).Identifier() || m_Descriptor.msaaSamples > 1)
		{
			depth = ShaderConstants._EdgeTexture;
			depthBufferBits = 24;
		}
		else
		{
			depth = sourceDepth;
			depthBufferBits = 0;
		}
		cmd.GetTemporaryRT(ShaderConstants._EdgeTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_SMAAEdgeFormat, depthBufferBits), FilterMode.Point);
		cmd.GetTemporaryRT(ShaderConstants._BlendTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8G8B8A8_UNorm), FilterMode.Point);
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(bufferRect);
		cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._EdgeTexture, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, depth, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
		cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 0);
		cmd.SetRenderTarget(new RenderTargetIdentifier(ShaderConstants._BlendTexture, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, depth, RenderBufferLoadAction.Load, RenderBufferStoreAction.DontCare);
		cmd.ClearRenderTarget(clearDepth: false, clearColor: true, Color.clear);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._EdgeTexture);
		cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 1);
		cmd.SetRenderTarget(new RenderTargetIdentifier(destination, 0, CubemapFace.Unknown, -1), RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.SetGlobalTexture(ShaderConstants._BlendTexture, ShaderConstants._BlendTexture);
		cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, subpixelMorphologicalAntialiasing, 0, 2);
		cmd.ReleaseTemporaryRT(ShaderConstants._EdgeTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._BlendTexture);
		cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
	}

	private void DoMotionBlur(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Expected I4, but got Unknown
		Material cameraMotionBlur = m_Materials.cameraMotionBlur;
		Matrix4x4 nonJitteredProjectionMatrix = camera.nonJitteredProjectionMatrix;
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 matrix4x = nonJitteredProjectionMatrix * worldToCameraMatrix;
		cameraMotionBlur.SetMatrix("_ViewProjM", matrix4x);
		if (m_ResetHistory)
		{
			cameraMotionBlur.SetMatrix("_PrevViewProjM", matrix4x);
		}
		else
		{
			cameraMotionBlur.SetMatrix("_PrevViewProjM", m_PrevViewProjM);
		}
		cameraMotionBlur.SetFloat("_Intensity", m_MotionBlur.intensity.value);
		cameraMotionBlur.SetFloat("_Clamp", m_MotionBlur.clamp.value);
		cmd.Blit(source, BlitDstDiscardContent(cmd, destination), cameraMotionBlur, (int)((VolumeParameter<MotionBlurQuality>)(object)m_MotionBlur.quality).value);
		m_PrevViewProjM = matrix4x;
	}

	private void SetupBloom(CommandBuffer cmd, RenderTargetIdentifier source, Material uberMaterial)
	{
		int num = m_Descriptor.width >> 1;
		int num2 = m_Descriptor.height >> 1;
		int num3 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Log(Mathf.Max(num, num2), 2f) - 1f), 1, 16);
		float value = m_Bloom.clamp.value;
		float num4 = Mathf.GammaToLinearSpace(m_Bloom.threshold.value);
		float w = num4 * 0.5f;
		float x = Mathf.Lerp(0.05f, 0.95f, m_Bloom.scatter.value);
		Material bloom = m_Materials.bloom;
		bloom.SetVector(ShaderConstants._Params, new Vector4(x, value, num4, w));
		CoreUtils.SetKeyword(bloom, "_BLOOM_HQ", m_Bloom.highQualityFiltering.value);
		CoreUtils.SetKeyword(bloom, "_USE_RGBM", m_UseRGBM);
		RenderTextureDescriptor stereoCompatibleDescriptor = GetStereoCompatibleDescriptor(num, num2, m_DefaultHDRFormat);
		cmd.GetTemporaryRT(ShaderConstants._BloomMipDown[0], stereoCompatibleDescriptor, FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._BloomMipUp[0], stereoCompatibleDescriptor, FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._BloomMipDown[0], bloom, 0);
		int num5 = ShaderConstants._BloomMipDown[0];
		for (int i = 1; i < num3; i++)
		{
			num = Mathf.Max(1, num >> 1);
			num2 = Mathf.Max(1, num2 >> 1);
			int num6 = ShaderConstants._BloomMipDown[i];
			int num7 = ShaderConstants._BloomMipUp[i];
			stereoCompatibleDescriptor.width = num;
			stereoCompatibleDescriptor.height = num2;
			cmd.GetTemporaryRT(num6, stereoCompatibleDescriptor, FilterMode.Bilinear);
			cmd.GetTemporaryRT(num7, stereoCompatibleDescriptor, FilterMode.Bilinear);
			cmd.Blit(num5, num7, bloom, 1);
			cmd.Blit(num7, num6, bloom, 2);
			num5 = num6;
		}
		for (int num8 = num3 - 2; num8 >= 0; num8--)
		{
			int num9 = ((num8 == num3 - 2) ? ShaderConstants._BloomMipDown[num8 + 1] : ShaderConstants._BloomMipUp[num8 + 1]);
			int num10 = ShaderConstants._BloomMipDown[num8];
			int num11 = ShaderConstants._BloomMipUp[num8];
			cmd.SetGlobalTexture(ShaderConstants._MainTexLowMip, num9);
			cmd.Blit(num10, BlitDstDiscardContent(cmd, num11), bloom, 3);
		}
		for (int j = 0; j < num3; j++)
		{
			cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipDown[j]);
			if (j > 0)
			{
				cmd.ReleaseTemporaryRT(ShaderConstants._BloomMipUp[j]);
			}
		}
		Color color = m_Bloom.tint.value.linear;
		float num12 = ColorUtils.Luminance(in color);
		color = ((num12 > 0f) ? (color * (1f / num12)) : Color.white);
		uberMaterial.SetVector(value: new Vector4(m_Bloom.intensity.value, color.r, color.g, color.b), nameID: ShaderConstants._Bloom_Params);
		uberMaterial.SetFloat(ShaderConstants._Bloom_RGBM, m_UseRGBM ? 1f : 0f);
		cmd.SetGlobalTexture(ShaderConstants._Bloom_Texture, ShaderConstants._BloomMipUp[0]);
		Texture texture = ((m_Bloom.dirtTexture.value == null) ? Texture2D.blackTexture : m_Bloom.dirtTexture.value);
		float num13 = (float)texture.width / (float)texture.height;
		float num14 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		Vector4 value2 = new Vector4(1f, 1f, 0f, 0f);
		float value3 = m_Bloom.dirtIntensity.value;
		if (num13 > num14)
		{
			value2.x = num14 / num13;
			value2.z = (1f - value2.x) * 0.5f;
		}
		else if (num14 > num13)
		{
			value2.y = num13 / num14;
			value2.w = (1f - value2.y) * 0.5f;
		}
		uberMaterial.SetVector(ShaderConstants._LensDirt_Params, value2);
		uberMaterial.SetFloat(ShaderConstants._LensDirt_Intensity, value3);
		uberMaterial.SetTexture(ShaderConstants._LensDirt_Texture, texture);
		if (m_Bloom.highQualityFiltering.value)
		{
			uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_HQ_DIRT" : "_BLOOM_HQ");
		}
		else
		{
			uberMaterial.EnableKeyword((value3 > 0f) ? "_BLOOM_LQ_DIRT" : "_BLOOM_LQ");
		}
	}

	private void SetupLensDistortion(Material material, bool isSceneView)
	{
		float b = 1.6f * Mathf.Max(Mathf.Abs(m_LensDistortion.intensity.value * 100f), 1f);
		float num = (float)Math.PI / 180f * Mathf.Min(160f, b);
		float y = 2f * Mathf.Tan(num * 0.5f);
		Vector2 vector = m_LensDistortion.center.value * 2f - Vector2.one;
		Vector4 value = new Vector4(vector.x, vector.y, Mathf.Max(m_LensDistortion.xMultiplier.value, 0.0001f), Mathf.Max(m_LensDistortion.yMultiplier.value, 0.0001f));
		Vector4 value2 = new Vector4((m_LensDistortion.intensity.value >= 0f) ? num : (1f / num), y, 1f / m_LensDistortion.scale.value, m_LensDistortion.intensity.value * 100f);
		material.SetVector(ShaderConstants._Distortion_Params1, value);
		material.SetVector(ShaderConstants._Distortion_Params2, value2);
		if (m_LensDistortion.IsActive() && !isSceneView)
		{
			material.EnableKeyword("_DISTORTION");
		}
	}

	private void SetupChromaticAberration(Material material)
	{
		material.SetFloat(ShaderConstants._Chroma_Params, m_ChromaticAberration.intensity.value * 0.05f);
		if (m_ChromaticAberration.IsActive())
		{
			material.EnableKeyword("_CHROMATIC_ABERRATION");
		}
	}

	private void SetupVignette(Material material)
	{
		Color value = m_Vignette.color.value;
		Vector2 value2 = m_Vignette.center.value;
		float num = (float)m_Descriptor.width / (float)m_Descriptor.height;
		if (m_IsStereo && XRGraphics.stereoRenderingMode == XRGraphics.StereoRenderingMode.SinglePass)
		{
			num *= 0.5f;
		}
		Vector4 value3 = new Vector4(value.r, value.g, value.b, m_Vignette.rounded.value ? num : 1f);
		Vector4 value4 = new Vector4(value2.x, value2.y, ((VolumeComponent)(object)m_Vignette).active ? (m_Vignette.intensity.value * 3f) : 0f, m_Vignette.smoothness.value * 5f);
		material.SetVector(ShaderConstants._Vignette_Params1, value3);
		material.SetVector(ShaderConstants._Vignette_Params2, value4);
	}

	private void SetupColorGrading(CommandBuffer cmd, ref RenderingData renderingData, Material material)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Invalid comparison between Unknown and I4
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Invalid comparison between Unknown and I4
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Invalid comparison between Unknown and I4
		ref PostProcessingData postProcessingData = ref renderingData.postProcessingData;
		bool flag = (int)postProcessingData.gradingMode == 1;
		int lutSize = postProcessingData.lutSize;
		int num = lutSize * lutSize;
		float w = Mathf.Pow(2f, m_ColorAdjustments.postExposure.value);
		cmd.SetGlobalTexture(ShaderConstants._InternalLut, Identifier(m_InternalLutID));
		material.SetVector(ShaderConstants._Lut_Params, new Vector4(1f / (float)num, 1f / (float)lutSize, (float)lutSize - 1f, w));
		material.SetTexture(ShaderConstants._UserLut, m_ColorLookup.texture.value);
		material.SetVector(ShaderConstants._UserLut_Params, (!m_ColorLookup.IsActive()) ? Vector4.zero : new Vector4(1f / (float)m_ColorLookup.texture.value.width, 1f / (float)m_ColorLookup.texture.value.height, (float)m_ColorLookup.texture.value.height - 1f, m_ColorLookup.contribution.value));
		if (flag)
		{
			material.EnableKeyword("_HDR_GRADING");
			return;
		}
		TonemappingMode value = ((VolumeParameter<TonemappingMode>)(object)m_Tonemapping.mode).value;
		if ((int)value != 1)
		{
			if ((int)value == 2)
			{
				material.EnableKeyword("_TONEMAP_ACES");
			}
		}
		else
		{
			material.EnableKeyword("_TONEMAP_NEUTRAL");
		}
	}

	private void SetupGrain(in CameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && m_FilmGrain.IsActive())
		{
			material.EnableKeyword("_FILM_GRAIN");
			PostProcessUtils.ConfigureFilmGrain(m_Data, m_FilmGrain, cameraData.camera.pixelWidth, cameraData.camera.pixelHeight, material);
		}
	}

	private void SetupDithering(in CameraData cameraData, Material material)
	{
		if (!m_HasFinalPass && cameraData.isDitheringEnabled)
		{
			material.EnableKeyword("_DITHERING");
			m_DitheringTextureIndex = PostProcessUtils.ConfigureDithering(m_Data, m_DitheringTextureIndex, cameraData.camera.pixelWidth, cameraData.camera.pixelHeight, material);
		}
	}

	private void RenderFinalPass(CommandBuffer cmd, ref RenderingData renderingData)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		SetupFinalPass(cmd, ref renderingData);
		ref CameraData cameraData = ref renderingData.cameraData;
		Material finalPass = m_Materials.finalPass;
		finalPass.shaderKeywords = null;
		if ((int)cameraData.antialiasing == 1)
		{
			finalPass.EnableKeyword("_FXAA");
		}
		SetupGrain(in cameraData, finalPass);
		SetupDithering(in cameraData, finalPass);
		if (RequireSRGBConversionBlitToBackBuffer(cameraData) && m_EnableSRGBConversionIfNeeded)
		{
			finalPass.EnableKeyword("_LINEAR_TO_SRGB_CONVERSION");
		}
		cmd.SetGlobalTexture("_BlitTex", sourceColor);
		RenderBufferLoadAction colorLoadAction = (cameraData.isDefaultViewport ? RenderBufferLoadAction.DontCare : RenderBufferLoadAction.Load);
		RenderTargetIdentifier rt = ((cameraData.targetTexture != null) ? new RenderTargetIdentifier(cameraData.targetTexture) : ((RenderTargetIdentifier)BuiltinRenderTextureType.CameraTarget));
		cmd.SetRenderTarget(rt, colorLoadAction, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
		if (((CameraData)(ref cameraData)).xr.enabled)
		{
			((ScriptableRenderPass)this).Blit(cmd, sourceColor, (RenderTargetIdentifier)BuiltinRenderTextureType.CurrentActive, finalPass, 0);
			return;
		}
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(bufferRect);
		cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, finalPass);
		cmd.SetViewProjectionMatrices(cameraData.camera.worldToCameraMatrix, cameraData.camera.projectionMatrix);
	}

	private void DoDepthOfField(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Rect pixelRect)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Invalid comparison between Unknown and I4
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		if ((int)((VolumeParameter<DepthOfFieldMode>)(object)m_DepthOfField.mode).value == 1)
		{
			DoGaussianDepthOfField(camera, cmd, source, destination, pixelRect);
		}
		else if ((int)((VolumeParameter<DepthOfFieldMode>)(object)m_DepthOfField.mode).value == 2)
		{
			DoBokehDepthOfField(cmd, source, destination, pixelRect);
		}
	}

	private void DoGaussianDepthOfField(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Rect pixelRect)
	{
		Material gaussianDepthOfField = m_Materials.gaussianDepthOfField;
		int num = m_Descriptor.width / 2;
		int height = m_Descriptor.height / 2;
		float value = m_DepthOfField.gaussianStart.value;
		float y = Mathf.Max(value, m_DepthOfField.gaussianEnd.value);
		float a = m_DepthOfField.gaussianMaxRadius.value * ((float)num / 1080f);
		a = Mathf.Min(a, 2f);
		CoreUtils.SetKeyword(gaussianDepthOfField, "_HIGH_QUALITY_SAMPLING", m_DepthOfField.highQualitySampling.value);
		gaussianDepthOfField.SetVector(ShaderConstants._CoCParams, new Vector3(value, y, a));
		cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, m_GaussianCoCFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._HalfCoCTexture, GetStereoCompatibleDescriptor(num, height, m_GaussianCoCFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetStereoCompatibleDescriptor(num, height, m_DefaultHDRFormat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetStereoCompatibleDescriptor(num, height, m_DefaultHDRFormat), FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._FullCoCTexture, gaussianDepthOfField, 0);
		m_MRT2[0] = ShaderConstants._HalfCoCTexture;
		m_MRT2[1] = ShaderConstants._PingTexture;
		cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
		cmd.SetViewport(pixelRect);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, source);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.SetRenderTarget(m_MRT2, ShaderConstants._HalfCoCTexture, 0, CubemapFace.Unknown, -1);
		cmd.DrawMesh(RenderingUtils.fullscreenMesh, Matrix4x4.identity, gaussianDepthOfField, 0, 1);
		cmd.SetViewProjectionMatrices(camera.worldToCameraMatrix, camera.projectionMatrix);
		cmd.SetGlobalTexture(ShaderConstants._HalfCoCTexture, ShaderConstants._HalfCoCTexture);
		cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, gaussianDepthOfField, 2);
		cmd.Blit(ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), gaussianDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._ColorTexture, ShaderConstants._PingTexture);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.Blit(source, BlitDstDiscardContent(cmd, destination), gaussianDepthOfField, 4);
		cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._HalfCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
	}

	private void PrepareBokehKernel()
	{
		if (m_BokehKernel == null)
		{
			m_BokehKernel = new Vector4[42];
		}
		int num = 0;
		float num2 = m_DepthOfField.bladeCount.value;
		float p = 1f - m_DepthOfField.bladeCurvature.value;
		float num3 = m_DepthOfField.bladeRotation.value * ((float)Math.PI / 180f);
		for (int i = 1; i < 4; i++)
		{
			float num4 = 1f / 7f;
			float num5 = ((float)i + num4) / (3f + num4);
			int num6 = i * 7;
			for (int j = 0; j < num6; j++)
			{
				float num7 = (float)Math.PI * 2f * (float)j / (float)num6;
				float num8 = Mathf.Cos((float)Math.PI / num2);
				float num9 = Mathf.Cos(num7 - (float)Math.PI * 2f / num2 * Mathf.Floor((num2 * num7 + (float)Math.PI) / ((float)Math.PI * 2f)));
				float num10 = num5 * Mathf.Pow(num8 / num9, p);
				float x = num10 * Mathf.Cos(num7 - num3);
				float y = num10 * Mathf.Sin(num7 - num3);
				m_BokehKernel[num] = new Vector4(x, y);
				num++;
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static float GetMaxBokehRadiusInPixels(float viewportHeight)
	{
		return Mathf.Min(0.05f, 14f / viewportHeight);
	}

	private void DoBokehDepthOfField(CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination, Rect pixelRect)
	{
		Material bokehDepthOfField = m_Materials.bokehDepthOfField;
		int num = m_Descriptor.width / 2;
		int num2 = m_Descriptor.height / 2;
		float num3 = m_DepthOfField.focalLength.value / 1000f;
		float num4 = m_DepthOfField.focalLength.value / m_DepthOfField.aperture.value;
		float value = m_DepthOfField.focusDistance.value;
		float y = num4 * num3 / (value - num3);
		float maxBokehRadiusInPixels = GetMaxBokehRadiusInPixels(m_Descriptor.height);
		float w = 1f / ((float)num / (float)num2);
		cmd.SetGlobalVector(ShaderConstants._CoCParams, new Vector4(value, y, maxBokehRadiusInPixels, w));
		int hashCode = ((object)m_DepthOfField).GetHashCode();
		if (hashCode != m_BokehHash)
		{
			m_BokehHash = hashCode;
			PrepareBokehKernel();
		}
		cmd.SetGlobalVectorArray(ShaderConstants._BokehKernel, m_BokehKernel);
		cmd.GetTemporaryRT(ShaderConstants._FullCoCTexture, GetStereoCompatibleDescriptor(m_Descriptor.width, m_Descriptor.height, GraphicsFormat.R8_UNorm), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PingTexture, GetStereoCompatibleDescriptor(num, num2, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
		cmd.GetTemporaryRT(ShaderConstants._PongTexture, GetStereoCompatibleDescriptor(num, num2, GraphicsFormat.R16G16B16A16_SFloat), FilterMode.Bilinear);
		cmd.Blit(source, ShaderConstants._FullCoCTexture, bokehDepthOfField, 0);
		cmd.SetGlobalTexture(ShaderConstants._FullCoCTexture, ShaderConstants._FullCoCTexture);
		cmd.Blit(source, ShaderConstants._PingTexture, bokehDepthOfField, 1);
		cmd.Blit(ShaderConstants._PingTexture, ShaderConstants._PongTexture, bokehDepthOfField, 2);
		cmd.Blit(ShaderConstants._PongTexture, BlitDstDiscardContent(cmd, ShaderConstants._PingTexture), bokehDepthOfField, 3);
		cmd.SetGlobalTexture(ShaderConstants._DofTexture, ShaderConstants._PingTexture);
		cmd.Blit(source, BlitDstDiscardContent(cmd, destination), bokehDepthOfField, 4);
		cmd.ReleaseTemporaryRT(ShaderConstants._FullCoCTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PingTexture);
		cmd.ReleaseTemporaryRT(ShaderConstants._PongTexture);
	}

	private void DoPaniniProjection(Camera camera, CommandBuffer cmd, RenderTargetIdentifier source, RenderTargetIdentifier destination)
	{
		float value = m_PaniniProjection.distance.value;
		Vector2 vector = CalcViewExtents(camera);
		Vector2 vector2 = CalcCropExtents(camera, value);
		float a = vector2.x / vector.x;
		float b = vector2.y / vector.y;
		float value2 = Mathf.Min(a, b);
		float num = value;
		float w = Mathf.Lerp(1f, Mathf.Clamp01(value2), m_PaniniProjection.cropToFit.value);
		Material paniniProjection = m_Materials.paniniProjection;
		paniniProjection.SetVector(ShaderConstants._Params, new Vector4(vector.x, vector.y, num, w));
		paniniProjection.EnableKeyword((1f - Mathf.Abs(num) > float.Epsilon) ? "_GENERIC" : "_UNIT_DISTANCE");
		cmd.Blit(source, BlitDstDiscardContent(cmd, destination), paniniProjection);
	}

	private Vector2 CalcViewExtents(Camera camera)
	{
		float num = camera.fieldOfView * ((float)Math.PI / 180f);
		float num2 = (float)m_Descriptor.width / (float)m_Descriptor.height;
		float num3 = Mathf.Tan(0.5f * num);
		return new Vector2(num2 * num3, num3);
	}

	private Vector2 CalcCropExtents(Camera camera, float d)
	{
		float num = 1f + d;
		Vector2 vector = CalcViewExtents(camera);
		float num2 = Mathf.Sqrt(vector.x * vector.x + 1f);
		float num3 = 1f / num2;
		float num4 = num3 + d;
		return vector * num3 * (num / num4);
	}

	private RenderTargetIdentifier Identifier(int id)
	{
		if (id == -1)
		{
			return BuiltinRenderTextureType.CameraTarget;
		}
		return new RenderTargetIdentifier(id);
	}
}
