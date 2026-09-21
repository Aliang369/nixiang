using System;
using System.Reflection;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

public class RenderingManager : GlobalGameManager
{
	public enum FrameRateMode
	{
		UnSet,
		HighFrameRate,
		BalanceFrameRate,
		LowFrameRate
	}

	public enum PerformanceMode
	{
		Unset = 0,
		Bad = 2,
		Normal = 3,
		Good = 4,
		Best = 5
	}

	public enum ResolutionMode
	{
		Unset = 0,
		Bad = 2,
		Normal = 3,
		Good = 4,
		Best = 5
	}

	public enum RendererId
	{
		Default,
		GameRenderer_NoPostProcess,
		GameRenderer_PostProcess
	}

	public enum PostProcessState
	{
		Disable,
		Fast,
		All
	}

	public FastPostProcessRenderFeature.FastPostProcessSettings defaultSettings;

	private Mesh envRenderMesh;

	private Sequence environmentRenderOverrideSequence;

	public static readonly Vector2Int BaseResolution = new Vector2Int(1920, 1080);

	private static bool VisualEffectsSupported => SystemInfo.supportsComputeShaders;

	public static float NormalResolutionAspectRatio => (float)BaseResolution.x / (float)BaseResolution.y;

	public static UniversalRenderPipelineAsset PipelineAsset
	{
		get
		{
			RenderPipelineAsset currentRenderPipeline = GraphicsSettings.currentRenderPipeline;
			return (UniversalRenderPipelineAsset)(object)((currentRenderPipeline is UniversalRenderPipelineAsset) ? currentRenderPipeline : null);
		}
	}

	public static ScriptableRendererData[] RenderDatas => typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(PipelineAsset) as ScriptableRendererData[];

	public static UniversalRendererData Renderer_envCenter_NoPostProcess
	{
		get
		{
			ScriptableRendererData obj = RenderDatas[1];
			return (UniversalRendererData)(object)((obj is UniversalRendererData) ? obj : null);
		}
	}

	public static UniversalRendererData Renderer_envCenter_PostProcess
	{
		get
		{
			ScriptableRendererData obj = RenderDatas[2];
			return (UniversalRendererData)(object)((obj is UniversalRendererData) ? obj : null);
		}
	}

	public static UniversalRendererData Renderer_final
	{
		get
		{
			ScriptableRendererData obj = RenderDatas[3];
			return (UniversalRendererData)(object)((obj is UniversalRendererData) ? obj : null);
		}
	}

	public static UniversalRendererData Renderer_env
	{
		get
		{
			ScriptableRendererData obj = RenderDatas[4];
			return (UniversalRendererData)(object)((obj is UniversalRendererData) ? obj : null);
		}
	}

	public SavingUtil.GlobalConfigSaveData GlobalConfigData => Bootstrap.instance.globalSaveConfigData;

	public FastPostProcessRenderFeature.FastPostProcessSettings? overrideSettings { get; private set; }

	public PerformanceMode CurrentPerformanceMode => GlobalConfigData.performanceMode;

	public bool EnableCameraShake => GlobalConfigData.enableCameraShake;

	public FrameRateMode CurrentFrameRateMode => GlobalConfigData.frameRateMode;

	private FinalRenderCamera FinalRenderCamera => Bootstrap.instance.finalCamera;

	private UniversalAdditionalCameraData envCenterCamera
	{
		get
		{
			FinalRenderCamera finalRenderCamera = FinalRenderCamera;
			if ((object)finalRenderCamera == null)
			{
				return null;
			}
			Camera environmentCenterCamera = finalRenderCamera.environmentCenterCamera;
			if ((object)environmentCenterCamera == null)
			{
				return null;
			}
			return CameraExtensions.GetUniversalAdditionalCameraData(environmentCenterCamera);
		}
	}

	public float EnvironmentRenderScale
	{
		get
		{
			return FinalRenderCamera.EnvRenderTextureScale;
		}
		set
		{
			FinalRenderCamera.EnvRenderTextureScale = value;
		}
	}

	public float EnvironmentCenterRenderScale
	{
		get
		{
			return FinalRenderCamera.EnvCenterRenderTextureScale;
		}
		set
		{
			FinalRenderCamera.EnvCenterRenderTextureScale = value;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		if (CurrentPerformanceMode == PerformanceMode.Unset)
		{
			if (PlatformHelper.IsMobile())
			{
				if (SystemInfo.systemMemorySize <= 4321)
				{
					GlobalConfigData.performanceMode = PerformanceMode.Bad;
					GlobalConfigData.resolutionMode = ResolutionMode.Bad;
					GlobalConfigData.frameRateMode = FrameRateMode.BalanceFrameRate;
				}
				else
				{
					GlobalConfigData.performanceMode = PerformanceMode.Normal;
					GlobalConfigData.resolutionMode = ResolutionMode.Normal;
					GlobalConfigData.frameRateMode = FrameRateMode.BalanceFrameRate;
				}
			}
			else
			{
				GlobalConfigData.performanceMode = PerformanceMode.Good;
				GlobalConfigData.resolutionMode = ResolutionMode.Good;
				GlobalConfigData.frameRateMode = FrameRateMode.HighFrameRate;
			}
		}
		GlobalConfigData.renderSettingsDirty = true;
		_OnGlobalConfigChanged(GlobalConfigData);
		RenderSettings.fogStartDistance = 8f;
		RenderSettings.fogEndDistance = 81f;
		applicationState.eventManager.GlobalConfigDataChanged.AddListener(_OnGlobalConfigChanged);
	}

	public override void Dispose()
	{
		environmentRenderOverrideSequence?.Kill();
		base.applicationState.eventManager.GlobalConfigDataChanged.RemoveListener(_OnGlobalConfigChanged);
	}

	private void ApplyResolutionMode(ResolutionMode mode)
	{
		if (mode != ResolutionMode.Unset && !((UnityEngine.Object)(object)envCenterCamera == null))
		{
			switch (mode)
			{
			case ResolutionMode.Bad:
				EnvironmentRenderScale = 0.4166f;
				EnvironmentCenterRenderScale = 0.4166f;
				break;
			case ResolutionMode.Normal:
				EnvironmentRenderScale = 0.6666f;
				EnvironmentCenterRenderScale = 0.6666f;
				break;
			case ResolutionMode.Good:
				EnvironmentRenderScale = 1f;
				EnvironmentCenterRenderScale = 1f;
				break;
			case ResolutionMode.Best:
				EnvironmentRenderScale = 2f;
				EnvironmentCenterRenderScale = 2f;
				break;
			default:
				throw new ArgumentOutOfRangeException("mode", mode, null);
			}
		}
	}

	private void ApplyFrameRateMode(FrameRateMode mode)
	{
		switch (mode)
		{
		case FrameRateMode.HighFrameRate:
			FinalRenderCamera.environmentCamera.GetComponent<CameraRenderInterval>().interval = 0;
			FinalRenderCamera.environmentCenterCamera.GetComponent<CameraRenderInterval>().interval = 0;
			break;
		case FrameRateMode.LowFrameRate:
			FinalRenderCamera.environmentCamera.GetComponent<CameraRenderInterval>().interval = 3;
			FinalRenderCamera.environmentCenterCamera.GetComponent<CameraRenderInterval>().interval = 2;
			break;
		default:
			FinalRenderCamera.environmentCamera.GetComponent<CameraRenderInterval>().interval = 1;
			FinalRenderCamera.environmentCenterCamera.GetComponent<CameraRenderInterval>().interval = 1;
			FinalRenderCamera.environmentCamera.GetComponent<CameraRenderInterval>().timer = 0;
			FinalRenderCamera.environmentCenterCamera.GetComponent<CameraRenderInterval>().timer = 1;
			break;
		}
	}

	private void ApplyPerformanceMode(PerformanceMode mode)
	{
		if (mode != PerformanceMode.Unset && !((UnityEngine.Object)(object)envCenterCamera == null))
		{
			RefreshPostProcessState();
			if (base.applicationState.runningGameState?.currentEncounter?.room != null)
			{
				ApplyPerformanceModeToDungeonRoom(base.applicationState.runningGameState.currentEncounter.room);
			}
			if (base.applicationState.runningGameState?.currentEncounter?.enemyAI != null)
			{
				ApplyPerformanceSettingToEnemy(base.applicationState.runningGameState.currentEncounter.enemyAI);
			}
		}
	}

	public void SetPostProcessState(PostProcessState state)
	{
		if (!((UnityEngine.Object)(object)envCenterCamera == null))
		{
			switch (state)
			{
			case PostProcessState.Disable:
				envCenterCamera.SetRenderer(1);
				envCenterCamera.renderPostProcessing = false;
				((Component)(object)envCenterCamera).GetComponent<Volume>().enabled = false;
				break;
			case PostProcessState.Fast:
				envCenterCamera.SetRenderer(2);
				envCenterCamera.renderPostProcessing = true;
				((Component)(object)envCenterCamera).GetComponent<Volume>().enabled = false;
				break;
			case PostProcessState.All:
				envCenterCamera.SetRenderer(2);
				envCenterCamera.renderPostProcessing = true;
				((Component)(object)envCenterCamera).GetComponent<Volume>().enabled = true;
				break;
			}
		}
	}

	public void OverrideFastPostProcessSettings(FastPostProcessRenderFeature.FastPostProcessSettings settings)
	{
		overrideSettings = settings;
		RefreshPostProcessState();
	}

	public void RestoreFastPostProcessSettings()
	{
		overrideSettings = null;
		RefreshPostProcessState();
	}

	public void ApplyEnvironmentRenderOverride(DungeonRoom room, EnvironmentInfluenceDef environmentInfluenceDef, bool transition = false)
	{
		environmentRenderOverrideSequence?.Kill();
		if (room == null)
		{
			return;
		}
		DungeonRoom.RoomRenderSettings renderSettings = room.renderSettings;
		EnvironmentInfluenceDef.EnvironmentRenderOverride environmentRenderOverride = environmentInfluenceDef?.renderOverride;
		bool flag = environmentRenderOverride?.enabled ?? false;
		Color color = MultiplyRgb(renderSettings.baseColorMultiply, flag ? environmentRenderOverride.baseColorMultiplier : 1f);
		Color color2 = MultiplyRgb(renderSettings.roomAmbient, flag ? environmentRenderOverride.roomAmbientMultiplier : 1f);
		Color color3 = MultiplyRgb(renderSettings.roomAmbient2, flag ? environmentRenderOverride.roomAmbient2Multiplier : 1f);
		float num = ((transition & flag) ? environmentRenderOverride.enterTransitionDuration : 0f);
		if (num <= 0f)
		{
			ApplyEnvironmentRenderValues(color, color2, color3);
			return;
		}
		Sequence sequence = DOTween.Sequence();
		sequence.Join(DOTween.To(() => CTRenderSetting.BaseColorMultiply, delegate(Color value)
		{
			CTRenderSetting.BaseColorMultiply = value;
		}, color, num).SetEase(Ease.InOutSine));
		sequence.Join(DOTween.To(() => CTRenderSetting.RoomAmbient, delegate(Color value)
		{
			CTRenderSetting.RoomAmbient = value;
		}, color2, num).SetEase(Ease.InOutSine));
		sequence.Join(DOTween.To(() => CTRenderSetting.RoomAmbient2, delegate(Color value)
		{
			CTRenderSetting.RoomAmbient2 = value;
		}, color3, num).SetEase(Ease.InOutSine));
		Sequence sequence2 = sequence;
		sequence2.onKill = (TweenCallback)Delegate.Combine(sequence2.onKill, (TweenCallback)delegate
		{
			if (environmentRenderOverrideSequence == sequence)
			{
				environmentRenderOverrideSequence = null;
			}
		});
		environmentRenderOverrideSequence = sequence;
		sequence.Play();
	}

	private static void ApplyEnvironmentRenderValues(Color baseColor, Color roomAmbient, Color roomAmbient2)
	{
		CTRenderSetting.BaseColorMultiply = baseColor;
		CTRenderSetting.RoomAmbient = roomAmbient;
		CTRenderSetting.RoomAmbient2 = roomAmbient2;
	}

	private static Color MultiplyRgb(Color color, float multiplier)
	{
		return new Color(color.r * multiplier, color.g * multiplier, color.b * multiplier, color.a);
	}

	private void RefreshPostProcessState()
	{
		if (CurrentPerformanceMode != PerformanceMode.Unset)
		{
			bool flag = overrideSettings?.chromaticAberration ?? false;
			PostProcessState postProcessState;
			switch (CurrentPerformanceMode)
			{
			case PerformanceMode.Bad:
			case PerformanceMode.Normal:
				postProcessState = (flag ? PostProcessState.Fast : PostProcessState.Disable);
				break;
			case PerformanceMode.Good:
				postProcessState = PostProcessState.Fast;
				break;
			case PerformanceMode.Best:
				postProcessState = PostProcessState.All;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			PostProcessState postProcessState2 = postProcessState;
			SetPostProcessState(postProcessState2);
		}
	}

	private void _OnGlobalConfigChanged(SavingUtil.GlobalConfigSaveData config)
	{
		if (GlobalConfigData.renderSettingsDirty)
		{
			GlobalConfigData.renderSettingsDirty = false;
			ApplyResolutionMode(config.resolutionMode);
			ApplyPerformanceMode(config.performanceMode);
			ApplyFrameRateMode(config.frameRateMode);
		}
	}

	public void ApplyPerformanceSettingToEnemy(CharacterAI enemy)
	{
		switch (CurrentPerformanceMode)
		{
		case PerformanceMode.Bad:
			SetOutlineEnable(enable: false);
			break;
		case PerformanceMode.Normal:
			SetOutlineEnable(enable: false);
			break;
		case PerformanceMode.Good:
			SetOutlineEnable(enable: true);
			break;
		case PerformanceMode.Best:
			SetOutlineEnable(enable: true);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PerformanceMode.Unset:
			break;
		}
		void SetOutlineEnable(bool enable)
		{
			foreach (DisplayUnit item in (enemy.BftDisplay?.displayUnits).OrEmptyIfNull())
			{
				if (item.outlineController?.outline != null)
				{
					item.outlineController.outline.gameObject.SetActive(enable);
				}
			}
		}
	}

	public void ApplyPerformanceModeToDungeonRoom(DungeonRoom room)
	{
		switch (CurrentPerformanceMode)
		{
		case PerformanceMode.Bad:
			SetVisualEffectEnable(enable: false);
			SetRoomNoiseFogEnable(enable: false);
			SetTerrainGrassDensity(0.2f);
			break;
		case PerformanceMode.Normal:
			SetVisualEffectEnable(enable: true);
			SetRoomNoiseFogEnable(enable: true);
			SetTerrainGrassDensity(0.5f);
			break;
		case PerformanceMode.Good:
			SetVisualEffectEnable(enable: true);
			SetRoomNoiseFogEnable(enable: true);
			SetTerrainGrassDensity(0.7f);
			break;
		case PerformanceMode.Best:
			SetVisualEffectEnable(enable: true);
			SetRoomNoiseFogEnable(enable: true);
			SetTerrainGrassDensity(1f);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case PerformanceMode.Unset:
			break;
		}
		void SetRoomNoiseFogEnable(bool enable)
		{
			SpriteRenderer[] componentsInChildren = room.GetComponentsInChildren<SpriteRenderer>();
			foreach (SpriteRenderer spriteRenderer in componentsInChildren)
			{
				if (spriteRenderer.sharedMaterial?.shader == ScriptableObjectSingleton<GameDataManager>.instance.noiseFogShader)
				{
					spriteRenderer.enabled = enable;
				}
			}
		}
		void SetTerrainGrassDensity(float density)
		{
			Terrain[] componentsInChildren = room.GetComponentsInChildren<Terrain>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].detailObjectDensity = density;
			}
		}
		void SetVisualEffectEnable(bool enable)
		{
			bool enabled = enable && VisualEffectsSupported;
			VisualEffect[] componentsInChildren = room.GetComponentsInChildren<VisualEffect>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				((Behaviour)(object)componentsInChildren[i]).enabled = enabled;
			}
			ParticleSystem[] componentsInChildren2 = room.GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem val in componentsInChildren2)
			{
				if (enable && !val.isPlaying)
				{
					val.Play();
				}
				if (!enable && val.isPlaying)
				{
					val.Stop();
				}
			}
		}
	}
}
