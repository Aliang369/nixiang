using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Webview;
using Common.UI;
using CoreKit.Agent;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingPanel : UIBase, IDisplayModulesController, IFrameUpdate
{
	[Serializable]
	public class PerformanceToggleSetup
	{
		public RenderingManager.PerformanceMode mode;

		public Toggle toggle;
	}

	[Serializable]
	public class ResolutionToggleSetup
	{
		public RenderingManager.ResolutionMode mode;

		public Toggle toggle;
	}

	public class DisplayModule : UIDisplayModule
	{
		public SettingPanel display => parent as SettingPanel;
	}

	[SerializeField]
	private Slider mainVolumeSlider;

	[SerializeField]
	private Slider musicVolumeSlider;

	[SerializeField]
	private Slider safeAreaLeft;

	[SerializeField]
	private Toggle timeScaleX1Toggle;

	[SerializeField]
	private Toggle timeScaleX2Toggle;

	[SerializeField]
	private Toggle timeScaleX4Toggle;

	[SerializeField]
	private Toggle enableCameraShakeToggle;

	[SerializeField]
	private Toggle disableCameraShakeToggle;

	[SerializeField]
	private Toggle enableQuickUseSkillToggle;

	[SerializeField]
	private Toggle disableQuickUseSkillToggle;

	[SerializeField]
	private Toggle disableAutoPlayButtonToggle;

	[SerializeField]
	private Toggle enableAutoPlayButtonToggle;

	[SerializeField]
	private Toggle enableAutoPlayAndEndTurnToggle;

	[SerializeField]
	private Toggle highFrameRateToggle;

	[SerializeField]
	private Toggle balancedFrameRateToggle;

	[SerializeField]
	private Toggle lowFrameRateToggle;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private GameButton customerServiceButton;

	public List<PerformanceToggleSetup> performanceToggles;

	public List<ResolutionToggleSetup> resolutionToggles;

	public SavingUtil.GlobalConfigSaveData GlobalConfigData => Bootstrap.instance.globalSaveConfigData;

	protected override void Init()
	{
		mainVolumeSlider.onValueChanged.AddListener(_OnMainVolumeSliderChange);
		mainVolumeSlider.value = GlobalConfigData.sfxVolume;
		musicVolumeSlider.onValueChanged.AddListener(_OnMusicVolumeSliderChange);
		musicVolumeSlider.value = GlobalConfigData.musicVolume;
		safeAreaLeft.onValueChanged.AddListener(_OnSafeAreaLeftSliderChange);
		safeAreaLeft.value = GlobalConfigData.safeAreaLeft;
		enableCameraShakeToggle.onValueChanged.AddListener(_OnEnableCameraShakeToggle);
		disableCameraShakeToggle.onValueChanged.AddListener(_OnDisableCameraShakeToggle);
		enableQuickUseSkillToggle.onValueChanged.AddListener(_OnEnableQuickUseSkillToggle);
		disableQuickUseSkillToggle.onValueChanged.AddListener(_OnDisableQuickUseSkillToggle);
		disableAutoPlayButtonToggle.onValueChanged.AddListener(_OnDisableAutoPlayButtonToggle);
		enableAutoPlayButtonToggle.onValueChanged.AddListener(_OnEnableAutoPlayButtonToggle);
		enableAutoPlayAndEndTurnToggle.onValueChanged.AddListener(_OnEnableAutoPlayAndEndTurnToggle);
		highFrameRateToggle.onValueChanged.AddListener(_OnHighFrameRateToggleToggle);
		balancedFrameRateToggle.onValueChanged.AddListener(_OnBalancedFrameRateToggleToggle);
		lowFrameRateToggle.onValueChanged.AddListener(_OnLowFrameRateToggleToggle);
		timeScaleX1Toggle.onValueChanged.AddListener(delegate(bool v)
		{
			_OnTimeScaleToggle(TimeScaleManager.TimeScaleType.X1, v);
		});
		timeScaleX2Toggle.onValueChanged.AddListener(delegate(bool v)
		{
			_OnTimeScaleToggle(TimeScaleManager.TimeScaleType.X2, v);
		});
		timeScaleX4Toggle.onValueChanged.AddListener(delegate(bool v)
		{
			_OnTimeScaleToggle(TimeScaleManager.TimeScaleType.X4, v);
		});
		foreach (PerformanceToggleSetup performanceSetup in performanceToggles)
		{
			performanceSetup.toggle.onValueChanged.AddListener(delegate(bool b)
			{
				_OnPerformanceModeToggle(b, performanceSetup);
			});
		}
		foreach (ResolutionToggleSetup resolutionSetup in resolutionToggles)
		{
			resolutionSetup.toggle.onValueChanged.AddListener(delegate(bool b)
			{
				_OnResolutionModeToggle(b, resolutionSetup);
			});
		}
		customerServiceButton.onClick.AddListener(_OpenCustomerService);
		SetDirty();
	}

	protected override void UpdateDisplay()
	{
		performanceToggles.FirstOrDefault((PerformanceToggleSetup t) => t.mode == GlobalConfigData.performanceMode)?.toggle.SetIsOnWithoutNotify(value: true);
		resolutionToggles.FirstOrDefault((ResolutionToggleSetup t) => t.mode == GlobalConfigData.resolutionMode)?.toggle.SetIsOnWithoutNotify(value: true);
		if (GlobalConfigData.enableCameraShake)
		{
			enableCameraShakeToggle.SetIsOnWithoutNotify(value: true);
		}
		else
		{
			disableCameraShakeToggle.SetIsOnWithoutNotify(value: true);
		}
		if (GlobalConfigData.quickUseSkill)
		{
			enableQuickUseSkillToggle.SetIsOnWithoutNotify(value: true);
		}
		else
		{
			disableQuickUseSkillToggle.SetIsOnWithoutNotify(value: true);
		}
		switch (GlobalConfigData.autoPlayButtonMode)
		{
		case AutoPlayButtonMode.Disabled:
			disableAutoPlayButtonToggle.SetIsOnWithoutNotify(value: true);
			break;
		case AutoPlayButtonMode.EnabledAndEndTurn:
			enableAutoPlayAndEndTurnToggle.SetIsOnWithoutNotify(value: true);
			break;
		default:
			enableAutoPlayButtonToggle.SetIsOnWithoutNotify(value: true);
			break;
		}
		switch (GlobalConfigData.battleTimeScaleTypeSetting)
		{
		case TimeScaleManager.TimeScaleType.X2:
			timeScaleX2Toggle.SetIsOnWithoutNotify(value: true);
			break;
		case TimeScaleManager.TimeScaleType.X4:
			timeScaleX4Toggle.SetIsOnWithoutNotify(value: true);
			break;
		default:
			timeScaleX1Toggle.SetIsOnWithoutNotify(value: true);
			break;
		}
		if (GlobalConfigData.frameRateMode == RenderingManager.FrameRateMode.LowFrameRate)
		{
			lowFrameRateToggle.SetIsOnWithoutNotify(value: true);
		}
		else if (GlobalConfigData.frameRateMode == RenderingManager.FrameRateMode.BalanceFrameRate)
		{
			balancedFrameRateToggle.SetIsOnWithoutNotify(value: true);
		}
		else
		{
			highFrameRateToggle.SetIsOnWithoutNotify(value: true);
		}
		versionText.text = Bootstrap.instance.bootstrapExtra.GetVersionText();
	}

	private void _OnMainVolumeSliderChange(float value)
	{
		GlobalConfigData.sfxVolume = value;
		GlobalConfigData.Save(UIBase.applicationState);
		Bootstrap.instance.audioManager.OnGlobalConfigChanged(GlobalConfigData);
	}

	private void _OnMusicVolumeSliderChange(float value)
	{
		GlobalConfigData.musicVolume = value;
		GlobalConfigData.Save(UIBase.applicationState);
		Bootstrap.instance.audioManager.OnGlobalConfigChanged(GlobalConfigData);
	}

	private void _OnSafeAreaLeftSliderChange(float value)
	{
		GlobalConfigData.safeAreaLeft = value;
		GlobalConfigData.Save(UIBase.applicationState);
		ResolutionSetter.refresh?.Invoke();
	}

	private void _OnTimeScaleToggle(TimeScaleManager.TimeScaleType type, bool value)
	{
		if (value)
		{
			GlobalConfigData.uiTimeScaleTypeSetting = type;
			GlobalConfigData.battleTimeScaleTypeSetting = type;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("加速模式", type.ToString(), UIBase.applicationState);
		}
	}

	private void _OnLowFrameRateToggleToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.frameRateMode = RenderingManager.FrameRateMode.LowFrameRate;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("帧率模式", GlobalConfigData.frameRateMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnBalancedFrameRateToggleToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.frameRateMode = RenderingManager.FrameRateMode.BalanceFrameRate;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("帧率模式", GlobalConfigData.frameRateMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnHighFrameRateToggleToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.frameRateMode = RenderingManager.FrameRateMode.HighFrameRate;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("帧率模式", GlobalConfigData.frameRateMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnEnableCameraShakeToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.enableCameraShake = true;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("相机摇动", GlobalConfigData.enableCameraShake.ToString(), UIBase.applicationState);
		}
	}

	private void _OnDisableCameraShakeToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.enableCameraShake = false;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("相机摇动", GlobalConfigData.enableCameraShake.ToString(), UIBase.applicationState);
		}
	}

	private void _OnEnableQuickUseSkillToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.quickUseSkill = true;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("快速使用护符", GlobalConfigData.quickUseSkill.ToString(), UIBase.applicationState);
		}
	}

	private void _OnDisableQuickUseSkillToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.quickUseSkill = false;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("快速使用护符", GlobalConfigData.quickUseSkill.ToString(), UIBase.applicationState);
		}
	}

	private void _OnDisableAutoPlayButtonToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.autoPlayButtonMode = AutoPlayButtonMode.Disabled;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("自动一键打出", GlobalConfigData.autoPlayButtonMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnEnableAutoPlayButtonToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.autoPlayButtonMode = AutoPlayButtonMode.Enabled;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("自动一键打出", GlobalConfigData.autoPlayButtonMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnEnableAutoPlayAndEndTurnToggle(bool value)
	{
		if (value)
		{
			GlobalConfigData.autoPlayButtonMode = AutoPlayButtonMode.EnabledAndEndTurn;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("自动一键打出", GlobalConfigData.autoPlayButtonMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnPerformanceModeToggle(bool value, PerformanceToggleSetup setup)
	{
		if (value)
		{
			GlobalConfigData.performanceMode = setup.mode;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("画质等级", GlobalConfigData.performanceMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OnResolutionModeToggle(bool value, ResolutionToggleSetup setup)
	{
		if (value)
		{
			GlobalConfigData.resolutionMode = setup.mode;
			GlobalConfigData.Save(UIBase.applicationState);
			ThinkingAnalyticsExtends.TrackSettingChange("分辨率等级", GlobalConfigData.resolutionMode.ToString(), UIBase.applicationState);
		}
	}

	private void _OpenCustomerService()
	{
		string customerFeedbackLink = GlobalConfigManager.instance.GetCustomerFeedbackLink();
		if (!string.IsNullOrEmpty(customerFeedbackLink))
		{
			ChillyRoomService.GetSdk().Core.WebViewClient.ShowOrCreateWebview(customerFeedbackLink, new OverrideWebviewSetting
			{
				WebviewOrientation = WebviewOrientation.Portrait
			});
			SetDirty();
		}
	}

	public void FrameUpdate()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !AgentHub.Instance.callCanShowExitDialog())
		{
			SetActiveAndDirtyWithFadeAnim(active: true);
		}
	}
}
