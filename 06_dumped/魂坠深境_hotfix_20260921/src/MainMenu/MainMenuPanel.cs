using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.RealName.Config;
using ChillyRoom.RealName.Model;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.Accounts;
using ChillyRoom.Services.Accounts.Sdk;
using ChillyRoom.Services.Accounts.Sdk.UI;
using ChillyRoom.Services.Accounts.Utils;
using ChillyRoom.Services.Core;
using CoreKit.Agent;
using CoreKit.Config;
using DG.Tweening;
using FMOD.Studio;
using Generated.ChillyRoomSdkClient;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainMenu;

public class MainMenuPanel : UIBase
{
	public enum LoginPhase
	{
		BeforeLogin,
		WaitLoginFinish,
		AfterLogin,
		AfterConfirmLogin
	}

	[SerializeField]
	private SettingPanel settingPanel;

	[SerializeField]
	private AnnouncementPanel announcementPanel;

	[SerializeField]
	private ChinaCompliancePanel chinaCompliancePanel;

	[SerializeField]
	public GameButton pressToStartButton;

	[SerializeField]
	private TextMeshProUGUI pressToStartText;

	[SerializeField]
	private TextMeshProUGUI customerServiceHintText;

	[SerializeField]
	private TextMeshProUGUI appMarkHintText;

	[SerializeField]
	private GameButton settingButton;

	[SerializeField]
	private GameButton announcementButton;

	[SerializeField]
	private GameButton openDisocrdButton;

	[SerializeField]
	private GameButton openSocialPromotionButton;

	[SerializeField]
	private CanvasGroup loginMethodArea;

	[SerializeField]
	private GameButton chillyroomMethodLoginButton;

	[SerializeField]
	private GameButton guestMethodLoginButton;

	[SerializeField]
	private GameButton appleGameCenterMethodLoginButton;

	[SerializeField]
	private GameButton tapTapLoginButton;

	[SerializeField]
	private TextMeshProUGUI hintMessageText;

	[SerializeField]
	private CanvasGroup hintMessageBox;

	[SerializeField]
	private Image maskImage;

	[NonSerialized]
	private Sequence seq;

	[NonSerialized]
	private EventInstance bgmInstance;

	[NonSerialized]
	public LoginPhase loginPhase;

	private GameConfig gameConfig => ChillyRoomService.GetSdk().Accounts.StateManager.Config;

	private List<IGrantType> loginGrantTypes => gameConfig.Distro.GrantTypes;

	protected override void Init()
	{
		InitUI(settingPanel);
		InitUI(announcementPanel);
		InitUI(chinaCompliancePanel);
		BootstrapExtra.instance.UpdateVersionText();
		pressToStartButton.onClick.AddListener(_OnPressToStart);
		settingButton.onClick.AddListener(_OnSettingButtonClicked);
		announcementButton.onClick.AddListener(_OnAnnouncementButtonClicked);
		openDisocrdButton.onClick.AddListener(_OnOpenDiscordLinkButtonClicked);
		openSocialPromotionButton.onClick.AddListener(_OnOpenSocialPromotionButtonClicked);
		chillyroomMethodLoginButton.onClick.AddListener(delegate
		{
			HideLoginMethodArea();
			LoginAsync();
		});
		guestMethodLoginButton.onClick.AddListener(delegate
		{
			HideLoginMethodArea();
			LoginAsync((UniversalSdkType)11);
		});
		appleGameCenterMethodLoginButton.onClick.AddListener(delegate
		{
			HideLoginMethodArea();
			LoginAsync((UniversalSdkType)12);
		});
		tapTapLoginButton.onClick.AddListener(delegate
		{
			HideLoginMethodArea();
			LoginAsync((UniversalSdkType)17);
		});
		SdkEvents.LoginResultAction -= _OnLoginResult;
		SdkEvents.LoginResultAction += _OnLoginResult;
		EventSystem.AddListener(NewUIEventEnum.CloseLoginView, ReturnToBeforeLogin);
		EventSystem.AddListener(NewUIEventEnum.CloseCloudSaveView, ReturnToBeforeLogin);
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Combine(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
		loginPhase = LoginPhase.BeforeLogin;
		guestMethodLoginButton.gameObject.SetActive(loginGrantTypes.Any((IGrantType g) => g.Type == "Guest"));
		appleGameCenterMethodLoginButton.gameObject.SetActive(loginGrantTypes.Any((IGrantType g) => g.Type == "Apple"));
		tapTapLoginButton.gameObject.SetActive(loginGrantTypes.Any((IGrantType g) => g.Type == "TapTap"));
		SyncMainMenuLocalizedTextData();
		RefreshMainMenuHintText();
	}

	protected override void Dispose()
	{
		SdkEvents.LoginResultAction -= _OnLoginResult;
		EventSystem.RemoveListener(NewUIEventEnum.CloseLoginView, ReturnToBeforeLogin);
		EventSystem.RemoveListener(NewUIEventEnum.CloseCloudSaveView, ReturnToBeforeLogin);
		LocalizationManager.OnLanguageChange = (Action<LocalizationManager.LanguageCode>)Delegate.Remove(LocalizationManager.OnLanguageChange, new Action<LocalizationManager.LanguageCode>(OnLanguageChanged));
	}

	public void StartMainMenu()
	{
		base.gameObject.SetActive(value: true);
		maskImage.gameObject.SetActive(value: false);
		openDisocrdButton.gameObject.SetActive(value: false);
		StartCoroutine(MainMenuCor());
	}

	private IEnumerator MainMenuCor()
	{
		HideLoginMethodArea();
		ThinkingAnalyticsExtends.TrackEnterLoginMenu();
		Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.bgm_menu_login, "BgmTag");
		ReturnToBeforeLogin();
		pressToStartText.DOFade(0f, 1f).SetLoops(-1, LoopType.Yoyo);
		pressToStartButton.gameObject.SetActive(value: false);
		float announcementWaitStartTime = Time.time;
		yield return new WaitUntil(() => announcementPanel.announcementInited || Time.time - announcementWaitStartTime >= 10f);
		if (!announcementPanel.announcementInited)
		{
			HintMessage("Load announcement failed");
		}
		pressToStartButton.gameObject.SetActive(value: true);
		announcementPanel.ShowIfHasNewAnnouncement();
		yield return new WaitUntil(() => loginPhase == LoginPhase.AfterLogin);
		AccountSdkUIFacade.GetAccountSdkUIFacade().AccountInformationBtn.gameObject.SetActive(value: true);
		yield return new WaitUntil(() => loginPhase == LoginPhase.AfterConfirmLogin);
		AccountSdkUIFacade.GetAccountSdkUIFacade().AccountInformationBtn.gameObject.SetActive(value: false);
		LoadAndStart(AccountStateManager.Instance.State.User.PlayerId.ToString());
	}

	private void Update()
	{
		RefreshMainMenuHintText();
	}

	private void OnLanguageChanged(LocalizationManager.LanguageCode _)
	{
		SyncMainMenuLocalizedTextData();
		RefreshMainMenuHintText();
	}

	private void SyncMainMenuLocalizedTextData()
	{
		LocalizationManager.LocalizeTarget(ScriptableObjectSingleton<TextDataPreset>.instance, new TraverseLocalizeTermState(TraverseLocalizeTermState.EType.Translate));
	}

	private void RefreshMainMenuHintText()
	{
		customerServiceHintText.text = GlobalConfigManager.instance.GetServerHint();
		appMarkHintText.text = GlobalConfigManager.instance.mainMenuAppMark;
	}

	public void HintMessage(string msg)
	{
		if (seq.IsActive())
		{
			seq.Complete();
		}
		seq = DOTween.Sequence();
		seq.AppendCallback(delegate
		{
			hintMessageText.text = msg;
			((Component)(object)hintMessageBox).gameObject.SetActive(value: true);
		});
		seq.Append(DOTweenModuleUI.DOFade(hintMessageBox, 1f, 0.1f));
		seq.AppendInterval(1f);
		seq.Append(DOTweenModuleUI.DOFade(hintMessageBox, 0f, 0.8f));
		seq.AppendCallback(delegate
		{
			((Component)(object)hintMessageBox).gameObject.SetActive(value: false);
		});
		seq.Play();
	}

	public void LoadAndStart(string playerID)
	{
		Bootstrap.instance.InitLoad(playerID, dropCurrentGame: false);
		base.gameObject.SetActive(value: false);
		ChillyRoomService.GetSdk().Accounts.HideUiView();
		ThinkingAnalyticsExtends.LoginTA(ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId.ToString());
		ChillyRoomBuglyIntegration.OnLogin(ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId.ToString());
	}

	private bool WithForbidAnnouncement()
	{
		foreach (AnnouncementButtonCell activeElement in announcementPanel.announcementButtonCellPrefabPool.GetActiveElements())
		{
			if (activeElement.setupData.AnnouncementKind == 0)
			{
				return true;
			}
		}
		return false;
	}

	private void _OnPressToStart()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		pressToStartButton.gameObject.SetActive(value: false);
		if (WithForbidAnnouncement())
		{
			announcementPanel.OpenAnnouncementPanel();
		}
		else if (loginPhase == LoginPhase.BeforeLogin)
		{
			loginPhase = LoginPhase.WaitLoginFinish;
			AccountSdkUIFacade.GetAccountSdkUIFacade();
			UpdateSDKLoginPrefabRectTransValue();
			if (ChillyRoomService.GetSdk().Accounts.HasSession())
			{
				UniversalSdkType? val = ChillyRoomService.GetSdk().Accounts.StateManager.Core.Cache.GetSdkStateCache()?.Session.LoginBy;
				if (TraceLog.EnabledLog)
				{
					TraceLog.Debug($"[MainMenuPanel] _OnPressToStart() - session exists, renew with cached type - {val}");
				}
				LoginAsync(val, true);
			}
			else
			{
				ShowLoginMethodArea();
			}
		}
		else if (loginPhase == LoginPhase.AfterLogin)
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] _OnPressToStart() - login finished, confirm login");
			}
			loginPhase = LoginPhase.AfterConfirmLogin;
		}
	}

	private async void LoginAsync(UniversalSdkType? type = null, bool? autoRenew = null)
	{
		HideLoginMethodArea();
		IResult<SdkState, Failure<AccountSdk.LoginErrorCode>> result;
		if (!type.HasValue)
		{
			result = await ChillyRoomService.GetSdk().Accounts.Login();
		}
		else
		{
			IResult<SdkState, Failure<AccountSdk.LoginErrorCode>> result2 = ((!autoRenew.HasValue) ? (await ChillyRoomService.GetSdk().Accounts.Login(type.Value)) : (await ChillyRoomService.GetSdk().Accounts.Login(type.Value, autoRenew.Value)));
			result = result2;
		}
		IResult<SdkState, Failure<AccountSdk.LoginErrorCode>> res = result;
		string loginTypeText = (type.HasValue ? ((object)type.GetValueOrDefault()/*cast due to constrained. prefix*/).ToString() : null) ?? "default";
		if (autoRenew.HasValue)
		{
			loginTypeText += $", autoRenew={autoRenew.Value}";
		}
		if (!res.IsOk())
		{
			ShowLoginMethodArea();
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug($"[MainMenuPanel] LoginAsync({loginTypeText}) - login failed - {res.Error}");
			}
			return;
		}
		if (type == (UniversalSdkType?)12)
		{
			await AuthorizeGameCenterAfterAppleLogin();
		}
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[MainMenuPanel] LoginAsync(" + loginTypeText + ") - login success - " + res.Data?.User?.Email + " " + res.Data?.User?.Phone);
		}
		HideLoginMethodArea();
	}

	private static async Task AuthorizeGameCenterAfterAppleLogin()
	{
		IosGameCenterAuthorization.Result result = await IosGameCenterAuthorization.AuthorizeAsync();
		if (result.IsAuthenticated)
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] AuthorizeGameCenterAfterAppleLogin() - Game Center authorization success - displayName=" + result.DisplayName + ", gamePlayerId=" + result.GamePlayerId);
			}
		}
		else if (!result.IsSkipped && TraceLog.EnabledLog)
		{
			TraceLog.Debug("[MainMenuPanel] AuthorizeGameCenterAfterAppleLogin() - Game Center authorization failed - " + result.ErrorMessage);
		}
	}

	private void ShowLoginMethodArea()
	{
		pressToStartButton.gameObject.SetActive(value: false);
		switch (loginGrantTypes.Count)
		{
		case 0:
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] ShowLoginMethodArea() - no available login grant type");
			}
			return;
		case 1:
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] ShowLoginMethodArea() - only one login grant type, login directly");
			}
			LoginAsync();
			return;
		}
		if (TraceLog.EnabledLog)
		{
			TraceLog.Debug("[MainMenuPanel] ShowLoginMethodArea() - multiple login grant types, show login method area");
		}
		loginMethodArea.alpha = 0f;
		((Component)(object)loginMethodArea).gameObject.SetActive(value: true);
		DOTweenModuleUI.DOFade(loginMethodArea, 1f, 0.2f).OnComplete(delegate
		{
			loginMethodArea.interactable = true;
			loginMethodArea.blocksRaycasts = true;
		});
	}

	private void HideLoginMethodArea()
	{
		((Component)(object)loginMethodArea).gameObject.SetActive(value: false);
	}

	private void _OnSettingButtonClicked()
	{
		settingPanel.SetActiveAndDirtyWithFadeAnim(active: true);
	}

	private void _OnAnnouncementButtonClicked()
	{
		announcementPanel.OpenAnnouncementPanel();
	}

	private void _OnOpenDiscordLinkButtonClicked()
	{
		Application.OpenURL(GlobalConfigManager.instance.discordGroupLink);
	}

	private void _OnOpenSocialPromotionButtonClicked()
	{
		ChillyRoomService.GetCore().WebViewClient.ShowOrCreateWebview(Bootstrap.instance.socialPromotionUrl);
	}

	private void UpdateSDKLoginPrefabRectTransValue()
	{
		RectTransform component = GlobalConfigManager.instance.replaceSDKLoginWindow.GetComponent<RectTransform>();
		RectTransform component2 = AccountSdkUIFacade.GetAccountSdkUIFacade().GetComponent<RectTransform>();
		RectTransform rectTransform = component.GetComponentsInChildren<RectTransform>(includeInactive: true).FirstOrDefault((RectTransform t) => t.gameObject.name == "AccountLoginContent");
		RectTransform rectTransform2 = component2.GetComponentsInChildren<RectTransform>(includeInactive: true).FirstOrDefault((RectTransform t) => t.gameObject.name == "AccountLoginContent");
		if (rectTransform != null && rectTransform2 != null)
		{
			SetRectTransValue(rectTransform, rectTransform2);
		}
		static void SetRectTransValue(RectTransform sourceRT, RectTransform instanceRT)
		{
			instanceRT.anchoredPosition = sourceRT.anchoredPosition;
			instanceRT.sizeDelta = sourceRT.sizeDelta;
		}
	}

	private unsafe async void _OnLoginResult(IResult<AccountSuccessResult, Failure<AccountFailResult>> result)
	{
		if (result.IsOk())
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] _OnLoginResult() - login success");
			}
			ChillyRoomService.GetSdk().AfterLogin();
			if (IsSpecialChannal())
			{
				RealNameConfigManager.Instance.SetAntiAddictionEnabled(false);
			}
			IResult<CheckRealNameStatusResult, CheckRealNameStatusError> result2 = await ChillyRoomService.GetSdk().RealName.CheckStatus(ServerTimePoller.Instance.GetCurrentTime());
			if (result2.IsOk())
			{
				if (TraceLog.EnabledLog)
				{
					TraceLog.Debug("[MainMenuPanel] _OnLoginResult() - real name check success");
				}
				if ((await ChillyRoomService.GetSdk().AntiAddiction.CheckStatus(ServerTimePoller.Instance.GetCurrentTime())).IsOk())
				{
					loginPhase = LoginPhase.AfterLogin;
					pressToStartButton.gameObject.SetActive(value: true);
					long playerId = ChillyRoomService.GetSdk().Accounts.StateManager.State.User.PlayerId;
					if (TraceLog.EnabledLog)
					{
						TraceLog.Debug($"[MainMenuPanel] _OnLoginResult() - anti addiction check success - playerId={playerId}");
					}
					BootstrapExtra.instance.UpdateVersionText();
					ThinkingAnalyticsExtends.LoginTA(playerId.ToString());
					ThinkingAnalyticsExtends.TrackLoginSuccess(Bootstrap.instance.globalSaveConfigData.game_active_times);
					long playerId2 = playerId;
					bool isNewUser = ChillyRoomService.GetSdk().Accounts.StateManager.State.User.IsNewUser;
					AccountSuccessResult data = result.Data;
					SolarEngineManager.OnLoginSuccess(playerId2, isNewUser, ((object)(*(UniversalSdkType*)(&data.SdkType))/*cast due to constrained. prefix*/).ToString());
					AgentHub.Instance.callEnterServer("1", "ContainerOfSoul");
					if (GlobalConfigManager.instance.CurrentDistroConfigInfo.agentHubCreateRoleCheckByPlayerPref)
					{
						if (PlayerPrefs.GetString("isFirstInstall", "true").Equals("true"))
						{
							PlayerPrefs.SetString("isFirstInstall", "false");
							AgentHub.Instance.callCreateRole(playerId.ToString(), "player");
						}
					}
					else if (ChillyRoomService.GetSdk().Accounts.StateManager.State.User.IsNewUser)
					{
						AgentHub.Instance.callCreateRole(playerId.ToString(), "player");
					}
					return;
				}
			}
			else if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] _OnLoginResult() - real name check failed - " + result2.Error.LocalizationMessage);
			}
		}
		else
		{
			if (TraceLog.EnabledLog)
			{
				TraceLog.Debug("[MainMenuPanel] _OnLoginResult() - login failed - " + result.Error.Description);
			}
			ReturnToBeforeLogin();
		}
		ReturnToBeforeLogin();
	}

	private void ReturnToBeforeLogin()
	{
		pressToStartButton.gameObject.SetActive(value: true);
		loginPhase = LoginPhase.BeforeLogin;
	}

	private bool IsSpecialChannal()
	{
		return !MinimalChillyRoomService.GetConfig().Distro.RealNameChannelConfig.isEnabled;
	}

	private void ApplyIOSChannelConfig()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Invalid comparison between Unknown and I4
		if ((int)CommonConfig.Instance.channelType != 3)
		{
			return;
		}
		foreach (IGrantType grantType in gameConfig.Distro.GrantTypes)
		{
			PasswordAuthGrantType val = (PasswordAuthGrantType)(object)((grantType is PasswordAuthGrantType) ? grantType : null);
			if (val != null)
			{
				if (!GlobalConfigManager.instance.isOverSea)
				{
					val.AllowLogin = (PasswordAuthAccountType)2;
					val.AllowRegister.AccountType = (PasswordAuthAccountType)1;
				}
				else
				{
					val.AllowLogin = (PasswordAuthAccountType)0;
					val.AllowRegister.AccountType = (PasswordAuthAccountType)0;
				}
			}
		}
	}
}
