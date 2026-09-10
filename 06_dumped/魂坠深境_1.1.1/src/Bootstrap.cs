using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChillyRoom.Announcement.V1;
using ChillyRoom.Privacy;
using ChillyRoom.RealName.Utils;
using ChillyRoom.Services.Accounts.Sdk;
using ChillyRoom.Services.ContainerOfSoul;
using ChillyRoom.Services.Core;
using FMODUnity;
using Generated.ChillyRoomSdkClient;
using MainMenu;
using Newtonsoft.Json;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Bootstrap : MonoBehaviour
{
	public static Bootstrap instance;

	public static Action init;

	[SerializeField]
	public GlobalConfigManager globalConfigManager;

	[SerializeField]
	public AssetReference envCameraRef;

	[SerializeField]
	public AssetReference bootstrapExtraRef;

	[SerializeField]
	public AssetReference assetCatalogRef;

	[SerializeField]
	public AssetReference MainMenuUIManagerRef;

	[NonSerialized]
	public AudioManager audioManager;

	[NonSerialized]
	public AdsManager adsManager;

	[NonSerialized]
	public ForceUpdateManager forceUpdateManager;

	[NonSerialized]
	public Camera initPlaceHolderCamera;

	[NonSerialized]
	public MainMenuUIManager mainMenuUIManager;

	[NonSerialized]
	public FinalRenderCamera finalCamera;

	[NonSerialized]
	public BootstrapExtra bootstrapExtra;

	[NonSerialized]
	public AssetCatalog assetCatalog;

	[NonSerialized]
	public SavingUtil.ProfileSaveData currentProfile;

	[NonSerialized]
	public string CurrentAccountID;

	[NonSerialized]
	public string socialPromotionUrl;

	[NonSerialized]
	public SavingUtil.GlobalConfigSaveData globalSaveConfigData;

	private bool unloadDraftMapsAfterSceneSwitch;

	public static ApplicationStateInfo applicationState => instance?.CurrentApplicationState;

	public static GameStateInfo gameState => applicationState?.runningGameState;

	public ApplicationStateInfo CurrentApplicationState
	{
		get
		{
			return currentProfile?.applicationState;
		}
		set
		{
			ApplicationStateInfo applicationStateInfo = currentProfile?.applicationState;
			if (applicationStateInfo != null && applicationStateInfo != value)
			{
				applicationStateInfo.routineQuestManager?.ClearPendingNetworkProgress("ApplicationStateChanged");
			}
			currentProfile.applicationState = value;
		}
	}

	private async void Start()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		instance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		PreInit();
		try
		{
			if (globalConfigManager.enableNetwork)
			{
				await InitChillyRoomSDKAsync();
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		WrapperEnumerator.StartCoroutine(BootstrapInitCor, this);
		SdkEvents.LogoutAction += _OnLogoutResult;
	}

	public void OnDestroy()
	{
		adsManager?.Dispose();
		forceUpdateManager.Dispose();
	}

	private void PreInit()
	{
		GameObject gameObject = new GameObject("initPlaceHolderCamera");
		gameObject.transform.SetParent(base.transform);
		initPlaceHolderCamera = gameObject.AddComponent<Camera>();
		initPlaceHolderCamera.backgroundColor = Color.black;
		initPlaceHolderCamera.clearFlags = CameraClearFlags.Color;
	}

	private IEnumerator BootstrapInitCor()
	{
		UIHelper.Init();
		init?.Invoke();
		Physics2D.simulationMode = (SimulationMode2D)2;
		Physics.simulationMode = (SimulationMode)2;
		Application.targetFrameRate = 60;
		TMP_Text.OnFontAssetRequest += (int _, string name) => ScriptableObjectSingleton<GameDataManager>.instance.allFontAssets.Find((TMP_FontAsset m) => m.name == name);
		Screen.autorotateToLandscapeLeft = true;
		Screen.autorotateToLandscapeRight = true;
		Screen.autorotateToPortrait = false;
		Screen.autorotateToPortraitUpsideDown = false;
		Screen.orientation = ScreenOrientation.AutoRotation;
		Application.backgroundLoadingPriority = ThreadPriority.High;
		AssetBundle.memoryBudgetKB = 5000u;
		QualitySettings.vSyncCount = 0;
		yield return envCameraRef.LoadAssetAsync<GameObject>().WaitForCompletion();
		finalCamera = UnityEngine.Object.Instantiate(envCameraRef.Asset as GameObject, base.transform).GetComponent<FinalRenderCamera>();
		finalCamera.gameObject.SetActive(value: false);
		ThinkingAnalyticsExtends.Init();
		SolarEngineManager.PreInit();
		if (CSCommon.isDebug)
		{
			SRDebug.Init();
		}
		yield return null;
		yield return bootstrapExtraRef.LoadAssetAsync<GameObject>().WaitForCompletion();
		bootstrapExtra = UnityEngine.Object.Instantiate(bootstrapExtraRef.Asset as GameObject, base.transform).GetComponent<BootstrapExtra>();
		bootstrapExtra.debugPanel.debugPanelTrigger.gameObject.SetActive(CSCommon.showDebugPanel);
		bootstrapExtra.gameObject.SetActive(value: true);
		initPlaceHolderCamera.gameObject.SetActive(value: false);
		UnityEngine.Object.Destroy(initPlaceHolderCamera);
		finalCamera.gameObject.SetActive(value: true);
		bootstrapExtra.bootstrapBackground.gameObject.SetActive(value: true);
		if (!CSCommon.enableUrpDebugRuntimeUI)
		{
			DebugManager.instance.enableRuntimeUI = false;
		}
		globalSaveConfigData = SavingUtil.LoadOrCreateGlobalSaveConfigData();
		ApplySavedLanguage(notifyLanguageChange: false);
		ThinkingAnalyticsExtends.TrackAppActive(globalSaveConfigData.game_active_times);
		globalSaveConfigData.game_active_times++;
		SavingUtil.SaveGlobalSaveConfigData(globalSaveConfigData);
		BootstrapType bootStrapType = CSCommon.bootStrapType;
		if ((int)bootStrapType == 0)
		{
			yield return LoadAssetCatalogCor();
			bootstrapExtra.bootstrapBackground.gameObject.SetActive(value: false);
		}
		else
		{
			yield return new WaitForSeconds(1f);
			bootstrapExtra.loadingPanel.Show(new LoadingPanel.LoadingShowInfo
			{
				showLoadingTitleText = true,
				loadingTitleText = CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_loading,
				loadingFinishedTitleText = CommonConfig.Instance.GetCurrentLauncherLocalization().Item2.term_loading_complete,
				loadingImage = globalConfigManager.loadingPanelConfig.defaultLoadingImage
			});
			yield return LoadAssetCatalogCor();
			bootstrapExtra.bootstrapBackground.gameObject.SetActive(value: false);
			bootstrapExtra.loadingPanel.NotifyProgress(1f);
			yield return new WaitUntil(() => !bootstrapExtra.loadingPanel.showing);
		}
		audioManager = new AudioManager();
		audioManager.Init();
		forceUpdateManager = new ForceUpdateManager();
		forceUpdateManager.Init();
		forceUpdateManager.StartCheckLoop();
		BootStrapLoadData bootStrapLoadData = (((int)bootStrapType != 0) ? BootStrapLoadData.ToMainMenu() : BootStrapLoadData.ToTestMenu());
		BootStrapLoadData bootStrapLoadData2 = bootStrapLoadData;
		bootStrapLoadData2.bootStrapInit = true;
		Load(bootStrapLoadData2);
	}

	public void SetCurrentProfile(string accountID, SavingUtil.ProfileSaveData profileData)
	{
		ApplicationStateInfo currentApplicationState = CurrentApplicationState;
		if (currentApplicationState != null && (CurrentAccountID != accountID || currentApplicationState != profileData?.applicationState))
		{
			currentApplicationState.routineQuestManager?.ClearPendingNetworkProgress("ProfileChanged");
		}
		CurrentAccountID = accountID;
		currentProfile = profileData;
	}

	public void SaveToCurrentProfileData(ApplicationStateInfo applicationState)
	{
		SavingUtil.SaveProfileData(applicationState, CurrentAccountID);
	}

	public Task SaveToCurrentProfileDataAsync(ApplicationStateInfo applicationState)
	{
		return SavingUtil.SaveProfileDataAsync(applicationState, CurrentAccountID);
	}

	private async Task InitChillyRoomSDKAsync()
	{
		if (MinimalChillyRoomService.GetMinimalSDK() == null)
		{
			await MinimalChillyRoomService.Bootstrap(CommonConfig.Instance.GetCurrentLauncherLocalization().Item1);
		}
		Debug.Log("InitChillyRoomSDK");
		AccountSdkUIFacade.CustomLoginWindowPrefab = GlobalConfigManager.instance.replaceSDKLoginWindow;
		ChillyRoomService chillyRoomService = ChillyRoomService.Bootstrap();
		chillyRoomService.AddAccountsIntegration().AddPaymentIntegration().AddCensorValidationIntegration()
			.AddRealNameIntegration()
			.AddMailIntegration()
			.AddAgreementIntegration()
			.AddMessagingIntegration();
		AccountSdkUIFacade.SetAutoShowRightBottom(isAutoShow: false);
		chillyRoomService.ContainerOfSoulIntegratioIn();
		socialPromotionUrl = await FetchAnnouncements(chillyRoomService, await GetServerTime());
		PrivacyResult privacyResult = await chillyRoomService.Agreement.Setup();
		TraceLog.Debug($"[Bootstrap] InitChillyRoomSDKAsync() - Agreement - isOK:{privacyResult.isOK} extra:{privacyResult.extra}");
		if (privacyResult.isOK)
		{
			ChillyRoomBuglyIntegration.Init();
		}
		else
		{
			Application.Quit();
		}
	}

	private async Task<DateTimeOffset> GetServerTime()
	{
		TraceLog.Debug("[Bootstrap] GetServerTime()");
		IResult<DateTimeOffset, Failure<GetUtcDateTimeErrorKind>> result = await MinimalChillyRoomService.GetCore().GetUtcDateTimeFromRemoteServerV2().Timeout(TimeSpan.FromSeconds(30.0))
			.Catch(delegate(TimeoutException e)
			{
				TraceLog.Error($"[Bootstrap] GetServerTime() - Time out - Exception: {e}");
				return Observable.Return(Result<DateTimeOffset, Failure<GetUtcDateTimeErrorKind>>.Ok(DateTimeOffset.Now));
			})
			.Catch(delegate(Exception e)
			{
				TraceLog.Error($"[Bootstrap] GetServerTime() - Exception: {e}");
				return Observable.Return(Result<DateTimeOffset, Failure<GetUtcDateTimeErrorKind>>.Err(new Failure<GetUtcDateTimeErrorKind>()));
			});
		if (!result.IsOk())
		{
			TraceLog.Error($"[Bootstrap] GetServerTime() - Get server time failed, use device time. \nException：{result.Error}");
			return DateTime.UtcNow;
		}
		DateTimeOffset data = result.Data;
		ServerTimePoller.Instance.Initialize(false, data, 60f, 60f);
		TraceLog.Debug($"[Bootstrap] GetServerTime()，ServerTime：{data}");
		return data;
	}

	private async Task<string> FetchAnnouncements(ChillyRoomService chillyRoomService, DateTimeOffset serverTime)
	{
		TraceLog.Debug($"[Bootstrap] FetchAnnouncements({serverTime})");
		IResult<List<AnnouncementOpenAPIModel>, Failure<FetchAnnouncementsErrorKind>> result = await chillyRoomService.Core.FetchAnnouncements(serverTime);
		if (!result.IsOk())
		{
			TraceLog.Error($"[Bootstrap] FetchAnnouncements({serverTime}) - Get announcements failed. \nException：{result.Error}");
			return null;
		}
		AnnouncementOpenAPIModel announcementOpenAPIModel = null;
		foreach (AnnouncementOpenAPIModel datum in result.Data)
		{
			TraceLog.Debug($"[Bootstrap] FetchAnnouncements() AnnouncementKind：{datum.AnnouncementKind}，AvailableSince：{datum.AvailableSince}");
			if (datum.AnnouncementKind == 20001 && (announcementOpenAPIModel == null || announcementOpenAPIModel.AvailableSince < datum.AvailableSince))
			{
				announcementOpenAPIModel = datum;
				TraceLog.Debug($"[Bootstrap] FetchAnnouncements()，Fetch social promotion data success - AvailableSince：{announcementOpenAPIModel.AvailableSince}");
			}
		}
		if (announcementOpenAPIModel == null)
		{
			TraceLog.Debug("[Bootstrap] FetchAnnouncements()，Get social promotion failed - no social promotion found");
			return null;
		}
		try
		{
			string openUrl = JsonConvert.DeserializeObject<SocialPromotionPage>(announcementOpenAPIModel.Payload).AnnouncementDatas.First().OpenUrl;
			TraceLog.Debug("[Bootstrap] FetchAnnouncements()，Get social promotion success - AvailableSince：" + socialPromotionUrl);
			return openUrl;
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[Bootstrap] FetchAnnouncements()，Get social promotion failed - Exception：{arg}");
		}
		return null;
	}

	public void InitLoad(string playerID, bool dropCurrentGame, bool skipGuide = false)
	{
		BootStrapLoadData bootStrapLoadData = BootStrapLoadData.ToAfterLoginLoad(playerID, dropCurrentGame, skipGuide);
		Load(bootStrapLoadData);
	}

	public void Load(BootStrapLoadData bootStrapLoadData)
	{
		WrapperEnumerator.StartCoroutine(() => BootstrapLoadCor(bootStrapLoadData), this);
	}

	private IEnumerator BootstrapLoadCor(BootStrapLoadData bootStrapLoadData)
	{
		bootstrapExtra.fortressReturnTransitionCover?.HideImmediate();
		if (!bootStrapLoadData.bootStrapInit && bootStrapLoadData.targetContext != BootStrapLoadData.TargetContext.TestMenu)
		{
			yield return ShowBootLoadingPanelCor(bootStrapLoadData);
		}
		CleanCurrentContext();
		if (ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization && bootStrapLoadData.targetContext != BootStrapLoadData.TargetContext.MainMenu)
		{
			LocalizationManager.LocalizeAssetCatalog();
		}
		if (GlobalConfigManager.instance.enableNetwork && !bootStrapLoadData.profilePlayerId.IsNullOrEmpty())
		{
			Task<bool> willCreateProfileTask = ChillyRoomService.GetSdk().ProfileData.WillCreatePlayerProfileDataAsync();
			yield return new WaitUntil(() => willCreateProfileTask.IsCompleted);
			if (willCreateProfileTask.IsFaulted || willCreateProfileTask.IsCanceled)
			{
				TraceLog.Error($"查询服务器是否将创建新存档失败: {willCreateProfileTask.Exception}");
				Load(BootStrapLoadData.ToMainMenu());
				yield break;
			}
			if (willCreateProfileTask.Result)
			{
				SavingUtil.CreateProfileData(bootStrapLoadData.profilePlayerId);
				WillCreatePlayerProfileDataHandleEvent();
			}
		}
		ReloadProfileDate(bootStrapLoadData);
		switch (bootStrapLoadData.targetContext)
		{
		case BootStrapLoadData.TargetContext.GameMap:
		case BootStrapLoadData.TargetContext.Fortress:
		case BootStrapLoadData.TargetContext.GameBattle:
		case BootStrapLoadData.TargetContext.AfterLoginLoad:
		case BootStrapLoadData.TargetContext.Intro:
			yield return LoadSceneCor(assetCatalog.applicationSceneRef);
			yield return LoadOrCreateApplicationStateCor(bootStrapLoadData);
			if (CurrentApplicationState == null)
			{
				yield break;
			}
			yield return CurrentApplicationState.PrepareStartCor();
			CurrentApplicationState.PrepareFortressReturnTransition(bootStrapLoadData);
			yield return HideBootLoadingPanelCor();
			yield return CurrentApplicationState.AfterLoadCor(bootStrapLoadData);
			break;
		case BootStrapLoadData.TargetContext.TestMenu:
			yield return LoadSceneCor(assetCatalog.testMenuSceneRef);
			break;
		case BootStrapLoadData.TargetContext.MainMenu:
			ApplySavedLanguage(notifyLanguageChange: true);
			yield return LoadSceneCor(assetCatalog.mainMenuSceneRef);
			if (mainMenuUIManager == null)
			{
				mainMenuUIManager = UnityEngine.Object.Instantiate(MainMenuUIManagerRef.Asset as GameObject, bootstrapExtra.chillyRoomViewsParent).GetComponent<MainMenuUIManager>();
				mainMenuUIManager.Init();
			}
			mainMenuUIManager.mainMenuPanel.StartMainMenu();
			yield return HideBootLoadingPanelCor();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		switch (bootStrapLoadData.targetContext)
		{
		case BootStrapLoadData.TargetContext.GameMap:
		case BootStrapLoadData.TargetContext.Fortress:
		case BootStrapLoadData.TargetContext.AfterLoginLoad:
			if (bootStrapLoadData.toGameScene == true)
			{
				if (bootStrapLoadData.IsNewGame)
				{
					ThinkingAnalyticsExtends.TrackApplicationStateContext(applicationState, "开始一局新游戏");
				}
				else
				{
					ThinkingAnalyticsExtends.TrackApplicationStateContext(applicationState, "从已有存档继续一局游戏");
				}
			}
			else if (bootStrapLoadData.toGameScene == false)
			{
				if (bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.AfterLoginLoad)
				{
					ThinkingAnalyticsExtends.TrackApplicationStateContext(applicationState, "登录进入主界面");
				}
				else
				{
					ThinkingAnalyticsExtends.TrackApplicationStateContext(applicationState, "结束游戏返回主界面");
				}
			}
			break;
		}
	}

	private void WillCreatePlayerProfileDataHandleEvent()
	{
		TraceLog.Debug("[Bootstrap] WillCreatePlayerProfileDataHandle");
		try
		{
			ThinkingAnalyticsExtends.TrackSignUp();
			TraceLog.Debug("[Bootstrap] WillCreatePlayerProfileDataHandle send event success.");
		}
		catch (Exception ex)
		{
			TraceLog.Error(ex.Message);
		}
	}

	private void ReloadProfileDate(BootStrapLoadData bootStrapLoadData)
	{
		if (!bootStrapLoadData.profilePlayerId.IsNullOrEmpty())
		{
			SavingUtil.ProfileSaveData profileData = SavingUtil.LoadOrCreateProfile(bootStrapLoadData.profilePlayerId);
			SetCurrentProfile(bootStrapLoadData.profilePlayerId, profileData);
		}
		else
		{
			SetCurrentProfile(null, null);
		}
	}

	private void ApplySavedLanguage(bool notifyLanguageChange)
	{
		if (globalSaveConfigData.languageCode == LocalizationManager.LanguageCode.Unset)
		{
			globalSaveConfigData.languageCode = LocalizationManager.SystemLanguageToLanguageCode(Application.systemLanguage);
		}
		LocalizationManager.LanguageCode languageCode = globalSaveConfigData.languageCode;
		if (assetCatalog != null)
		{
			List<LocalizationManager.LanguageCode> localLanguages = ScriptableObjectSingleton<LocalizationManager>.instance.localLanguages;
			if (localLanguages == null || !localLanguages.Contains(languageCode))
			{
				TraceLog.Warn($"当前语言 {languageCode} 没有本地化数据，回退到英文。系统语言：{Application.systemLanguage}");
				languageCode = LocalizationManager.LanguageCode.English;
				globalSaveConfigData.languageCode = languageCode;
				SavingUtil.SaveGlobalSaveConfigData(globalSaveConfigData);
			}
		}
		if (notifyLanguageChange && ScriptableObjectSingleton<LocalizationManager>.instance.EnableLocalization)
		{
			ScriptableObjectSingleton<LocalizationManager>.instance.SetLanguage(languageCode);
		}
		else
		{
			LocalizationManager.SetRawLanguage(languageCode);
		}
	}

	private IEnumerator LoadSceneCor(AssetReference sceneRef)
	{
		AsyncOperationHandle<SceneInstance> handle = Addressables.LoadSceneAsync(sceneRef);
		yield return new WaitUntil(() => handle.Task.IsCompleted);
		if (handle.Status == AsyncOperationStatus.Succeeded && unloadDraftMapsAfterSceneSwitch)
		{
			assetCatalog.draftMapCatalog.UnloadAll();
			unloadDraftMapsAfterSceneSwitch = false;
		}
	}

	private void CleanCurrentContext()
	{
		unloadDraftMapsAfterSceneSwitch = CurrentApplicationState?.runningGameState != null;
		if (mainMenuUIManager != null)
		{
			UnityEngine.Object.Destroy(mainMenuUIManager.gameObject);
			mainMenuUIManager = null;
		}
		if (assetCatalog.fortressManagerRef.IsValid())
		{
			assetCatalog.fortressManagerRef.ReleaseAsset();
		}
		if (assetCatalog.gameManagerRef.IsValid())
		{
			assetCatalog.gameManagerRef.ReleaseAsset();
		}
		assetCatalog.roomInfoDataCatalog.UnloadAll();
		assetCatalog.enemyInfoDataCatalog.UnloadAll();
		assetCatalog.cutsceneCatalog.UnloadAll();
		CurrentApplicationState?.Dispose();
	}

	private IEnumerator ShowBootLoadingPanelCor(BootStrapLoadData bootStrapLoadData, float minDuration = 2f)
	{
		LoadingPanel.LoadingShowInfo loadingShowInfo;
		if (bootStrapLoadData.loadingTargetLevel != null)
		{
			loadingShowInfo = new LoadingPanel.LoadingShowInfo
			{
				showLoadingTitleText = true,
				loadingLevel = bootStrapLoadData.loadingTargetLevel
			};
			ContractPresentationDef.StagePresentation stagePresentation = bootStrapLoadData.newGameConfigData?.contract?.contractDef?.presentation?.GetStagePresentation(bootStrapLoadData.loadingTargetLevel);
			if (stagePresentation != null)
			{
				loadingShowInfo.loadingTitleText = stagePresentation.loadingTitle;
				loadingShowInfo.loadingImage = stagePresentation.loadingImage;
			}
		}
		else if (bootStrapLoadData.newGameConfigData?.contract != null)
		{
			Contract contract = bootStrapLoadData.newGameConfigData.contract;
			loadingShowInfo = new LoadingPanel.LoadingShowInfo
			{
				showLoadingTitleText = true,
				loadingLevel = contract.GetInitLevel(assetCatalog)
			};
			ContractPresentationDef.StagePresentation stagePresentation2 = contract.contractDef?.presentation?.GetStagePresentation(loadingShowInfo.loadingLevel);
			if (stagePresentation2 != null)
			{
				loadingShowInfo.loadingTitleText = stagePresentation2.loadingTitle;
				loadingShowInfo.loadingImage = stagePresentation2.loadingImage;
			}
		}
		else
		{
			LoadingPanel.Config loadingPanelConfig = GlobalConfigManager.instance.loadingPanelConfig;
			loadingShowInfo = new LoadingPanel.LoadingShowInfo
			{
				showLoadingTitleText = false,
				loadingImage = loadingPanelConfig.defaultLoadingImage
			};
		}
		if (CurrentApplicationState?.guideManager != null)
		{
			NewGameConfigData newGameConfigData = bootStrapLoadData.newGameConfigData;
			if (newGameConfigData == null || !newGameConfigData.ToIntro)
			{
				loadingShowInfo.loadingHintText = CurrentApplicationState.guideManager.GenerateGuideHint();
			}
		}
		loadingShowInfo.minDuration = minDuration;
		BootstrapExtra.instance.loadingPanel.Show(loadingShowInfo);
		yield return new WaitForSeconds(GlobalConfigManager.instance.loadingPanelConfig.showTime);
	}

	private IEnumerator LoadAssetCatalogCor()
	{
		if (GlobalConfigManager.instance.createPredefinedProfiles)
		{
			AccountUtil.SetupPredefinedProfiles();
		}
		if (assetCatalog == null)
		{
			yield return assetCatalogRef.LoadAssetAsync<AssetCatalog>();
			assetCatalog = assetCatalogRef.Asset as AssetCatalog;
		}
		yield return MainMenuUIManagerRef.LoadAssetAsync<GameObject>();
		ApplySavedLanguage(notifyLanguageChange: true);
		TraceLog.Debug("RuntimeManager - LoadBank()");
		ScriptableObjectSingleton<AudioData>.instance.GlobalLoadBanks.ForEach(delegate(string bank)
		{
			RuntimeManager.LoadBank(bank, false);
		});
	}

	private IEnumerator HideBootLoadingPanelCor()
	{
		if (BootstrapExtra.instance.loadingPanel.showing)
		{
			BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
			yield return new WaitUntil(() => !BootstrapExtra.instance.loadingPanel.showing);
		}
	}

	private IEnumerator LoadOrCreateApplicationStateCor(BootStrapLoadData bootStrapLoadData)
	{
		if (CurrentApplicationState == null)
		{
			CurrentApplicationState = new ApplicationStateInfo();
		}
		UnityEngine.Object.FindObjectOfType<ApplicationBoot>().Setup(CurrentApplicationState);
		CurrentApplicationState.Init(bootStrapLoadData);
		yield return new WaitUntil(() => CurrentApplicationState.Inited);
		yield return FetchPlayerData();
		if (CurrentApplicationState != null)
		{
			yield return CurrentApplicationState.PostFetchData();
		}
	}

	private IEnumerator FetchPlayerData()
	{
		if (!GlobalConfigManager.instance.enableNetwork)
		{
			yield break;
		}
		GeneralGameAsyncOperation<FetchProfileDataReq, PlayerProfileData> FetchProfileDataOp = new GeneralGameAsyncOperation<FetchProfileDataReq, PlayerProfileData>(new FetchProfileDataReq
		{
			DateTimeUtcOffsetOfMinutes = (int)DateTimeOffset.Now.Offset.TotalMinutes
		}, ChillyRoomService.GetSdk().ProfileData.FetchPlayerProfileDataAsync);
		FetchProfileDataOp.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_player_data);
		yield return FetchProfileDataOp.WaitForEnd();
		if (!FetchProfileDataOp.isComplete)
		{
			CurrentApplicationState = null;
			Load(BootStrapLoadData.ToMainMenu());
			yield break;
		}
		PlayerProfileData opResult = FetchProfileDataOp.OpResult;
		try
		{
			CurrentApplicationState.ApplyProfileData(opResult);
		}
		catch (Exception arg)
		{
			TraceLog.Error($"[Bootstrap] FetchPlayerData() - {arg}");
		}
		List<ChillyRoom.Services.ContainerOfSoul.GachaPoolState> gachaPoolStates = opResult.GachaPoolStates;
		if (gachaPoolStates != null && gachaPoolStates.Count > 0 && opResult.GachaPoolStates.All((ChillyRoom.Services.ContainerOfSoul.GachaPoolState state) => state.DrawRewardHistory != null))
		{
			Debug.LogError("[Bootstrap] FetchPlayerData() - 触发已废弃逻辑");
			CurrentApplicationState.gachaManager.MarkLoadedHistoriesReady();
		}
		else
		{
			GeneralGameAsyncOperationResultOnly<GachaManager.GachaHistorySyncBatch> fetchGachaHistoryOp = CurrentApplicationState.gachaManager.CreateHistorySynchronizationOperation();
			fetchGachaHistoryOp.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_gacha_history);
			yield return fetchGachaHistoryOp.WaitForEnd();
			if (!fetchGachaHistoryOp.isComplete)
			{
				CurrentApplicationState.gachaManager.MarkAllHistorySynchronizationFailed();
				CurrentApplicationState = null;
				Load(BootStrapLoadData.ToMainMenu());
				yield break;
			}
			if (!CurrentApplicationState.gachaManager.ApplyHistorySyncBatch(fetchGachaHistoryOp.OpResult))
			{
				TraceLog.Error("登录抽卡历史同步结果无效");
				CurrentApplicationState = null;
				Load(BootStrapLoadData.ToMainMenu());
				yield break;
			}
		}
		yield return CurrentApplicationState.RequestSplitProfileDataAfterFetchPlayerData();
		GameAsyncOperation gameAsyncOperation = CurrentApplicationState.limitedTimeActivityManager.CreateFetchRuntimeConfigsOperation();
		gameAsyncOperation.EnqueueRun(GameAsyncOperationLane.DuringLoading, ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_activity_config);
		yield return gameAsyncOperation.WaitForEnd();
		yield return CurrentApplicationState.purchaseManager.RequestSkuList();
		Task<bool> t = CurrentApplicationState.purchaseManager.RestoreOrders();
		yield return new WaitUntil(() => t.IsCompleted);
		yield return applicationState.matterManager.SyncMatter();
		if (CurrentApplicationState.intermissionPlayManager.TryFetchInfo(forceRefresh: true))
		{
			yield return new WaitUntil(() => !CurrentApplicationState.intermissionPlayManager.IsFetchingInfo);
		}
	}

	public static void SetProfileDirtyToSave()
	{
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	private void _OnLogoutResult(bool s)
	{
		CurrentApplicationState?.routineQuestManager?.ClearPendingNetworkProgress("Logout");
		BootstrapExtra.instance.gameAsyncOperationManager.Clear();
		Load(BootStrapLoadData.ToMainMenu());
	}
}
