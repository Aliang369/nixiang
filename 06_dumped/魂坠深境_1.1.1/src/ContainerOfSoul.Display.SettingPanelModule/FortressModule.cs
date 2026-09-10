using ChillyRoom.Services.Accounts.Sdk;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;

namespace ContainerOfSoul.Display.SettingPanelModule;

public class FortressModule : SettingPanel.DisplayModule
{
	private enum ConfirmPanelMode
	{
		None,
		Logout,
		Language
	}

	[SerializeField]
	private GameButton resumeButton;

	[SerializeField]
	private GameButton logoutButton;

	[SerializeField]
	private GameButton privacyButton;

	[SerializeField]
	private GameButton giftCodeButton;

	[SerializeField]
	private GameButton purchaseRecordButton;

	[SerializeField]
	private GameButton umpButton;

	[SerializeField]
	private TextMeshProUGUI accountInfoText;

	[SerializeField]
	private TextMeshProUGUI nickNameText;

	[SerializeField]
	private TextMeshProUGUI realNameInfoText;

	[SerializeField]
	private TextMeshProUGUI versionText;

	[SerializeField]
	private LanguageSettingArea languageSettingArea;

	[SerializeField]
	private TextMeshProUGUI accountStatus;

	[SerializeField]
	private GameObject ConfirmLogoutPanel;

	[SerializeField]
	private GameButton ConfirmLogoutButton;

	[SerializeField]
	private GameButton CancelLogoutButton;

	[SerializeField]
	private GameObject logoutConfirmHint;

	[SerializeField]
	private GameObject languageConfirmHint;

	[SerializeField]
	private GameButton bindAccountButton;

	[SerializeField]
	private TextMeshProUGUI cheatScoreText;

	private LocalizationManager.LanguageCode pendingLanguageCode;

	private ConfirmPanelMode confirmPanelMode;

	public override void Init()
	{
		if (languageSettingArea != null)
		{
			base.display.InitUI(languageSettingArea);
			languageSettingArea.SetLanguageChangeRequestHandler(_OnLanguageChangeRequested);
			languageSettingArea.gameObject.SetActive(GlobalConfigManager.instance.enableLocalization);
		}
		resumeButton.onClick.AddListener(_OnResumeButtonClicked);
		logoutButton.onClick.AddListener(_OnLogoutButtonClicked);
		ConfirmLogoutButton.onClick.AddListener(_OnConfirmButtonClicked);
		CancelLogoutButton.onClick.AddListener(_OnCancelConfirmButtonClicked);
		privacyButton.onClick.AddListener(_OnPrivacyButtonClicked);
		giftCodeButton.onClick.AddListener(_OnGiftCodeButtonClicked);
		giftCodeButton.onClick.AddListener(delegate
		{
			UIDisplayModule.fortressState.uiManager.fortressGiftCodePanel.ShowUI();
			ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIDisplayModule.applicationState, "通用-礼包码");
		});
		purchaseRecordButton.onClick.AddListener(delegate
		{
			UIDisplayModule.fortressState.uiManager.purchaseRecordPanel.ShowUI();
		});
		bindAccountButton.onClick.AddListener(delegate
		{
			ThinkingAnalyticsExtends.TrackAccountBinding(1);
			ChillyRoomService.GetSdk().Accounts.BindEmail();
		});
		Bootstrap.instance.adsManager.InitUmpButton(umpButton);
	}

	public override void UpdateDisplay()
	{
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		if (languageSettingArea != null)
		{
			languageSettingArea.gameObject.SetActive(GlobalConfigManager.instance.enableLocalization);
		}
		accountInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_account_arg0.FormatWith(Bootstrap.instance.CurrentAccountID);
		nickNameText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_current_nickname_arg0.FormatWith(UIDisplayModule.applicationState.playerInfoManager.DisplayName);
		cheatScoreText.text = ((UIDisplayModule.applicationState.playerInfoManager.cheatScore > 0) ? UIDisplayModule.applicationState.playerInfoManager.cheatScore.ToString() : string.Empty);
		versionText.text = $"{AppVersionDisplay.GetDisplayVersion()}  [{CSCommon.packageType}-{CSCommon.netWorkEndPointType}-{CSCommon.channelType}-{CSCommon.version}-{CSCommon.versionCode}]";
		if (!GlobalConfigManager.instance.isOverSea)
		{
			realNameInfoText.gameObject.SetActive(value: true);
			realNameInfoText.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_real_name_applied;
		}
		else
		{
			realNameInfoText.gameObject.SetActive(value: false);
		}
		giftCodeButton.gameObject.SetActive(UIDisplayModule.applicationState.giftCodeManager.GiftCodeEnabled);
		UpdateAccountStatus();
		void UpdateAccountStatus()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Invalid comparison between Unknown and I4
			bindAccountButton.gameObject.SetActive(value: false);
			SdkState sdkState = UIDisplayModule.applicationState.playerInfoManager.sdkState;
			User user = sdkState?.User;
			if (sdkState == null || user == null)
			{
				accountStatus.text = "";
			}
			else if ((int)sdkState.CurrentLoginType == 11 && user.Phone.IsNullOrEmpty() && user.Email.IsNullOrEmpty())
			{
				bindAccountButton.gameObject.SetActive(value: true);
				accountStatus.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_guest_account;
			}
			else if (!user.Phone.IsNullOrEmpty())
			{
				accountStatus.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_phone_account.FormatWith(user.Phone);
			}
			else if (!user.Email.IsNullOrEmpty())
			{
				accountStatus.text = ScriptableObjectSingleton<TextDataPreset>.instance.term_email_account.FormatWith(user.Email);
			}
			else
			{
				accountStatus.text = "";
			}
		}
	}

	private void _OnResumeButtonClicked()
	{
		UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		base.display.SetActiveAndDirtyWithFadeAnim(active: false);
	}

	private void _OnLogoutButtonClicked()
	{
		confirmPanelMode = ConfirmPanelMode.Logout;
		SetConfirmHintMode(ConfirmPanelMode.Logout);
		ConfirmLogoutPanel.gameObject.SetActive(value: true);
	}

	private void _OnConfirmButtonClicked()
	{
		switch (confirmPanelMode)
		{
		case ConfirmPanelMode.Logout:
			ConfirmLogout();
			break;
		case ConfirmPanelMode.Language:
			ApplyPendingLanguageAndReload();
			break;
		default:
			HideConfirmPanel();
			break;
		}
	}

	private void ConfirmLogout()
	{
		UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		AccountUtil.Logout();
	}

	private void _OnCancelConfirmButtonClicked()
	{
		if (confirmPanelMode == ConfirmPanelMode.Language && languageSettingArea != null)
		{
			languageSettingArea.RestoreCurrentLanguageSelection();
		}
		pendingLanguageCode = LocalizationManager.LanguageCode.Unset;
		HideConfirmPanel();
	}

	private void HideConfirmPanel()
	{
		confirmPanelMode = ConfirmPanelMode.None;
		ConfirmLogoutPanel.gameObject.SetActive(value: false);
		SetConfirmHintMode(ConfirmPanelMode.Logout);
	}

	private void _OnLanguageChangeRequested(LocalizationManager.LanguageCode code)
	{
		confirmPanelMode = ConfirmPanelMode.Language;
		pendingLanguageCode = code;
		SetConfirmHintMode(ConfirmPanelMode.Language);
		ConfirmLogoutPanel.gameObject.SetActive(value: true);
	}

	private void ApplyPendingLanguageAndReload()
	{
		if (pendingLanguageCode == LocalizationManager.LanguageCode.Unset)
		{
			_OnCancelConfirmButtonClicked();
			return;
		}
		LocalizationManager.LanguageCode languageCode = pendingLanguageCode;
		pendingLanguageCode = LocalizationManager.LanguageCode.Unset;
		HideConfirmPanel();
		UIDisplayModule.applicationState.savingManager.SetProfileDirtyToSave();
		Bootstrap.instance.globalSaveConfigData.languageCode = languageCode;
		Bootstrap.instance.globalSaveConfigData.Save();
		Bootstrap.instance.Load(BootStrapLoadData.ToMainMenu());
	}

	private void SetConfirmHintMode(ConfirmPanelMode mode)
	{
		if (logoutConfirmHint != null)
		{
			logoutConfirmHint.SetActive(mode == ConfirmPanelMode.Logout);
		}
		if (languageConfirmHint != null)
		{
			languageConfirmHint.SetActive(mode == ConfirmPanelMode.Language);
		}
	}

	private void _OnPrivacyButtonClicked()
	{
		if (!GlobalConfigManager.instance.isOverSea)
		{
			ChillyRoomService.GetSdk().Agreement.ShowPrivacyPage();
		}
		else
		{
			Application.OpenURL(GlobalConfigManager.instance.PrivacyPolicyLink);
		}
	}

	private void _OnGiftCodeButtonClicked()
	{
		UIDisplayModule.fortressState.uiManager.fortressGiftCodePanel.ShowUI();
		ThinkingAnalyticsExtends.TrackOpenFortressSubPanel(UIDisplayModule.applicationState, "通用-礼包码");
	}
}
