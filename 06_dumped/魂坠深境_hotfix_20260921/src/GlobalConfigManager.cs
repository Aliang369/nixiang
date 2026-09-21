using System;
using System.Collections.Generic;
using System.Linq;
using CoreKit.Util;
using Sirenix.OdinInspector;
using UnityEngine;

public class GlobalConfigManager : SerializedScriptableObject
{
	public class DistroConfigInfo
	{
		public string DistroId;

		public string distroName;

		public bool reviewNeedCustomPopup;

		public bool agentHubCreateRoleCheckByPlayerPref;

		public string onlineArea;

		public bool isCreatorDistro;

		public string ShareLink;
	}

	public class PredefinedProfile
	{
		public string accountID;

		public string nickName;

		public string password;

		public TextAsset profileData;
	}

	private static GlobalConfigManager _instance;

	public PlatformHelper.Type debugPlatformType;

	public bool disableBakeRoomLoad;

	public string reportLink;

	public string discordLink;

	public string benefitsInfoLink;

	public string googlePlayCustomerFeedbackLink;

	public string nonGooglePlayCustomerFeedbackLink;

	public string qqGroupLink;

	public string discordGroupLink;

	public string PrivacyPolicyLink;

	public bool createPredefinedProfiles;

	public List<PredefinedProfile> predefinedProfiles;

	[TextArea]
	public string mainMenuAppMark;

	public bool enableLocalization;

	public string saveVersion;

	public int localSaveVersion;

	public bool enableEditorTA;

	public bool debugLogTA;

	public LoadingPanel.Config loadingPanelConfig;

	public UISafeArea.Config safeAreaConfig;

	public GameObject replaceSDKLoginWindow;

	[LocalizeText("渠道配置信息")]
	public List<DistroConfigInfo> distroConfigInfos;

	[NonSerialized]
	private bool _isOnlineAreaChina;

	public static GlobalConfigManager instance
	{
		get
		{
			if (_instance != null)
			{
				return _instance;
			}
			if (Bootstrap.instance != null && Bootstrap.instance.globalConfigManager != null)
			{
				_instance = Bootstrap.instance.globalConfigManager;
			}
			return _instance;
		}
	}

	public bool enableNetwork => CSCommon.enableNetwork;

	public CommonConfig commonConfig => CommonConfig.Instance;

	public DistroConfigInfo CurrentDistroConfigInfo
	{
		get
		{
			if (MinimalChillyRoomService.GetMinimalSDK() == null)
			{
				return null;
			}
			return distroConfigInfos?.FirstOrDefault((DistroConfigInfo d) => d?.DistroId == MinimalChillyRoomService.GetConfig().Distro.Id);
		}
	}

	public bool EnableBlockWordsCheck
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			return (int)commonConfig.netWorkEndPointType == 3;
		}
	}

	public bool isOverSea
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Invalid comparison between Unknown and I4
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Invalid comparison between Unknown and I4
			if ((int)commonConfig.channelType == 2)
			{
				return true;
			}
			if ((int)commonConfig.channelType == 3 && !isDeviceChinaMainland)
			{
				return true;
			}
			return false;
		}
	}

	private bool isDeviceChinaMainland => AreaDetectUtil.IsChinaMainLand((CountryDetectionStrategy)2);

	public bool showCustomReviewPopup
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Invalid comparison between Unknown and I4
			return (int)commonConfig.channelType == 10;
		}
	}

	public string GetCustomerFeedbackLink()
	{
		if (!isOverSea)
		{
			return nonGooglePlayCustomerFeedbackLink;
		}
		return googlePlayCustomerFeedbackLink;
	}

	public bool IsCurrentDistroGooglePlay()
	{
		if (MinimalChillyRoomService.GetMinimalSDK() == null)
		{
			return false;
		}
		string text = MinimalChillyRoomService.GetConfig()?.Distro?.Id;
		if (!string.Equals(text, "Google", StringComparison.OrdinalIgnoreCase))
		{
			return (text?.IndexOf("GooglePlay", StringComparison.OrdinalIgnoreCase) ?? (-1)) >= 0;
		}
		return true;
	}

	public string GetServerHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.GetMainMenuCustomerSupportHint();
	}
}
