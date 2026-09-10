using System;
using System.Collections;
using System.Collections.Generic;
using Common.UI;
using ContainerOfSoul.Display.Fortress;
using CoreKit.Agent;
using FMODUnity;
using Generated.ChillyRoomSdkClient;
using UnityEngine;

public class ApplicationStateInfo
{
	private const string DifficultySelectionClavisId = "#CLA#000029#";

	[SaveField(null, null)]
	public GlobalUnlockManager globalUnlockManager;

	[SaveField(null, null)]
	public PlayerGlobalInfoManager playerInfoManager;

	[SaveField(null, null)]
	public AchievementManager achievementManager;

	[SaveField(null, null)]
	public AnalyticsObserverManager analyticsObserverManager;

	[SaveField(null, null)]
	public CharacterManager characterManager;

	[SaveField(null, null)]
	public ClavisManager clavisManager;

	[SaveField(null, null)]
	public SkillGemManager skillGemManager;

	[SaveField(null, null)]
	public SeasonManager seasonManager;

	[SaveField(null, null)]
	public ContractManager contractManager;

	[SaveField(null, null)]
	public DifficultyManager difficultyManager;

	[SaveField(null, null)]
	public DiscoverManager discoverManager;

	[SaveField(null, null)]
	public GameStatisticsManager gameStatisticsManager;

	[SaveField(null, null)]
	public GlobalSkillManager globalSkillManager;

	[SaveField(null, null)]
	public ApplicationGlobalVariableManager globalVariableManager;

	[SaveField(null, null)]
	public GuideManager guideManager;

	[SaveField(null, null)]
	public MatterManager matterManager;

	[SaveField(null, null)]
	public NewGameConfigManager newGameConfigManager;

	[SaveField(null, null)]
	public OpenTestManager openTestManager;

	[SaveField(null, null)]
	public PlayTimeRecordManager playTimeRecordManager;

	[SaveField(null, null)]
	public GlobalGameRecordManager recordManager;

	[SaveField(null, null)]
	public StoryPointManager storyPointManager;

	[SaveField(null, null)]
	public ForerunnerManager forerunnerManager;

	[SaveField(null, null)]
	public RenderingManager renderingManager;

	[SaveField(null, null)]
	public PurchaseManager purchaseManager;

	[SaveField(null, null)]
	public IdolManager idolManager;

	[SaveField(null, null)]
	public RoutineQuestManager routineQuestManager;

	[SaveField(null, null)]
	public DailyCheckManager dailyCheckManager;

	[SaveField(null, null)]
	public ItemSkinManager itemSkinManager;

	[SaveField(null, null)]
	public StartGameBuffManager startGameBuffManager;

	[SaveField(null, null)]
	public TimeScaleManager settingManager;

	[SaveField(null, null)]
	public RankingManager rankingManager;

	[SaveField(null, null)]
	public GachaManager gachaManager;

	[SaveField(null, null)]
	public MailManager mailManager;

	[SaveField(null, null)]
	public GiftCodeManager giftCodeManager;

	[SaveField(null, null)]
	public PleaseReviewPopupManager pleaseReviewPopupManager;

	[NonSerialized]
	public DataMigrationManager dataMigrationManager;

	[SaveField(null, null)]
	public YuanQiActivityManager yuanQiActivityManager;

	[SaveField(null, null)]
	public NFXManager nfxManager;

	[SaveField(null, null)]
	public PVPModeManager pvpModeManager;

	[SaveField(null, null)]
	public PlayerReportManager playerReportManager;

	[SaveField(null, null)]
	public CharacterTrialManager characterTrialManager;

	public LimitedTimeActivityManager limitedTimeActivityManager;

	[SaveField(null, null)]
	public IntermissionPlayManager intermissionPlayManager;

	[SaveField(null, null)]
	public NewbieFestivalManager newbieFestivalManager;

	[SaveField(null, null)]
	public GameStateInfo savedGameState;

	[SaveField(null, null)]
	public LastGameEndReason lastGameEndReason;

	[SaveField(null, null)]
	public bool testUnlockAllGranted;

	[SaveField(null, null)]
	public bool firstLoginTracked;

	[SaveField(null, null)]
	private bool _showUnlockCharacter = true;

	public ApplicationUIManager applicationUIManager;

	public VirtualCamera currentVirtualCamera;

	public EventManager eventManager;

	public ExceptionTraceManager exceptionTraceManager;

	public SavingManager savingManager;

	public AssetCatalog assetCatalog;

	public BootStrapLoadData bootStrapLoadData;

	public FortressStateInfo fortressState;

	public GameStateInfo runningGameState;

	public PlayCutSceneState currentCutSceneState;

	public bool aiSessionMode;

	public AiGameSessionController aiGameSessionController;

	[NonSerialized]
	private FortressChallengeReturnTransition fortressReturnTransition;

	private readonly List<GlobalGameManager> subManagers = new List<GlobalGameManager>();

	public bool inited;

	public bool showUnlockCharacter
	{
		get
		{
			return _showUnlockCharacter;
		}
		set
		{
			if (_showUnlockCharacter != value)
			{
				_showUnlockCharacter = value;
				savingManager.SetProfileDirtyToSave();
			}
		}
	}

	public bool Inited
	{
		get
		{
			if (inited)
			{
				return subManagers.TrueForAll((GlobalGameManager x) => x.Inited);
			}
			return false;
		}
	}

	public VFXDataPreset vfxDataPreset => ScriptableObjectSingleton<VFXDataPreset>.instance;

	public ChillyRoomService ChillyRoomSDK => ChillyRoomService.GetSdk();

	public void Init(BootStrapLoadData bootStrapLoadData)
	{
		TraceLog.Initialize();
		this.bootStrapLoadData = bootStrapLoadData;
		assetCatalog = Bootstrap.instance.assetCatalog;
		if (Bootstrap.instance.adsManager == null)
		{
			Bootstrap.instance.adsManager = new AdsManager();
			Bootstrap.instance.adsManager.Init();
		}
		InitSubManager(ref eventManager);
		InitSubManager(ref savingManager);
		InitSubManager(ref settingManager);
		InitSubManager(ref playTimeRecordManager);
		InitSubManager(ref exceptionTraceManager);
		InitSubManager(ref globalVariableManager);
		InitSubManager(ref renderingManager);
		InitSubManager(ref contractManager);
		InitSubManager(ref newGameConfigManager);
		InitSubManager(ref characterManager);
		InitSubManager(ref recordManager);
		InitSubManager(ref globalUnlockManager);
		InitSubManager(ref matterManager);
		InitSubManager(ref achievementManager);
		InitSubManager(ref gameStatisticsManager);
		InitSubManager(ref discoverManager);
		InitSubManager(ref globalSkillManager);
		InitSubManager(ref seasonManager);
		InitSubManager(ref routineQuestManager);
		InitSubManager(ref dailyCheckManager);
		InitSubManager(ref guideManager);
		InitSubManager(ref difficultyManager);
		InitSubManager(ref idolManager);
		InitSubManager(ref analyticsObserverManager);
		InitSubManager(ref openTestManager);
		InitSubManager(ref skillGemManager);
		InitSubManager(ref itemSkinManager);
		InitSubManager(ref startGameBuffManager);
		InitSubManager(ref clavisManager);
		InitSubManager(ref storyPointManager);
		InitSubManager(ref forerunnerManager);
		InitSubManager(ref purchaseManager);
		InitSubManager(ref rankingManager);
		InitSubManager(ref gachaManager);
		InitSubManager(ref mailManager);
		InitSubManager(ref giftCodeManager);
		InitSubManager(ref pleaseReviewPopupManager);
		InitSubManager(ref dataMigrationManager);
		InitSubManager(ref yuanQiActivityManager);
		InitSubManager(ref pvpModeManager);
		InitSubManager(ref playerReportManager);
		InitSubManager(ref characterTrialManager);
		InitSubManager(ref limitedTimeActivityManager);
		InitSubManager(ref intermissionPlayManager);
		InitSubManager(ref newbieFestivalManager);
		InitSubManager(ref playerInfoManager);
		InitSubManager(ref nfxManager);
		matterManager.PostInit();
		newGameConfigManager.SanitizeLimitedDeploymentSelection();
		if (bootStrapLoadData.startUnlockAll && bootStrapLoadData.targetContext != BootStrapLoadData.TargetContext.TestMenu && !testUnlockAllGranted)
		{
			TestUnlockAll();
			testUnlockAllGranted = true;
			savingManager.SetProfileDirtyToSave();
		}
		inited = true;
		ThinkingAnalyticsExtends.SetUser(this);
		if (!firstLoginTracked)
		{
			firstLoginTracked = true;
			savingManager.SetProfileDirtyToSave();
			ThinkingAnalyticsExtends.TrackLoginFirstSuccess();
		}
	}

	public IEnumerator PostFetchData()
	{
		foreach (GlobalGameManager subManager in subManagers)
		{
			if (subManager is IPostFetchData postFetchData)
			{
				yield return postFetchData.PostFetchData();
			}
		}
	}

	public IEnumerator PrepareStartCor()
	{
		achievementManager.InitAllAchievementObserve();
		routineQuestManager.InitAllRoutineQuestObserve();
		if (bootStrapLoadData.completeIntroBeforeFirstGame)
		{
			PrepareFirstGameAfterCompletedIntroForTest();
		}
		bool flag = bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.Intro || (bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.AfterLoginLoad && !guideManager.introFinished);
		bootStrapLoadData.toIntro = flag;
		bool toGameScene = !bootStrapLoadData.dropCurrentGame && (((bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.GameMap || bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.GameBattle) | flag) || (bootStrapLoadData.targetContext == BootStrapLoadData.TargetContext.AfterLoginLoad && savedGameState != null));
		bootStrapLoadData.toGameScene = toGameScene;
		if (bootStrapLoadData.dropCurrentGame)
		{
			NewGameConfigData finishedRunConfig = savedGameState?.newGameConfigData;
			ThinkingAnalyticsExtends.TrackDropLastSaveStartGame(savedGameState != null);
			if (savedGameState != null)
			{
				RecordLastGameEndReason(LastGameEndReason.ReturnToLobby);
			}
			savedGameState = null;
			newGameConfigManager.ResetAfterLimitedDeploymentRun(finishedRunConfig);
			savingManager.SaveProfile();
		}
		if (flag)
		{
			bootStrapLoadData.newGameConfigData = NewGameConfigData.Intro(assetCatalog);
		}
		foreach (RoomRef commonRoomRef in ScriptableObjectSingleton<FortressDataManager>.instance.commonRoomRefs)
		{
			if (!commonRoomRef.IsLoaded(assetCatalog))
			{
				yield return commonRoomRef.StartLoad(assetCatalog);
			}
		}
		AgentHub.Instance.callEnterGame(globalUnlockManager.globalLevelLine.CurrentDisplayLevel.ToString(), "player", bootStrapLoadData.profilePlayerId);
		if (toGameScene)
		{
			if (!assetCatalog.gameManagerRef.IsValid())
			{
				yield return assetCatalog.gameManagerRef.LoadAssetAsync<GameObject>();
			}
			ScriptableObjectSingleton<AudioData>.instance.InGameBanks.ForEach(delegate(string bank)
			{
				RuntimeManager.LoadBank(bank, false);
			});
			ScriptableObjectSingleton<AudioData>.instance.FortressBanks.ForEach(delegate(string bank)
			{
				RuntimeManager.UnloadBank(bank);
			});
			runningGameState = (bootStrapLoadData.IsNewGame ? new GameStateInfo() : savedGameState);
			if (runningGameState != null)
			{
				if (!bootStrapLoadData.IsNewGame)
				{
					RunningGameSaveMigration.Migrate(runningGameState);
				}
				runningGameState.startBootStrapData = bootStrapLoadData;
				BootSetter<GameStateInfo>.BootAttach(assetCatalog.gameManagerRef.Asset as GameObject, runningGameState);
				runningGameState.Init(this);
				yield return new WaitUntil(() => runningGameState.Inited);
				yield return runningGameState.StartGameCor();
				if (!runningGameState.IsCharacterTrialGame)
				{
					ThinkingAnalyticsExtends.TrackGameStart(runningGameState, bootStrapLoadData.IsNewGame, runningGameState.applicationState.guideManager.RunningIntroModule != null);
				}
				runningGameState.applicationState.eventManager.gameEnterEvent.TriggerEvent();
			}
			yield break;
		}
		if (!assetCatalog.fortressManagerRef.IsValid())
		{
			yield return assetCatalog.fortressManagerRef.LoadAssetAsync<GameObject>();
		}
		ScriptableObjectSingleton<AudioData>.instance.FortressBanks.ForEach(delegate(string bank)
		{
			RuntimeManager.LoadBank(bank, false);
		});
		ScriptableObjectSingleton<AudioData>.instance.InGameBanks.ForEach(delegate(string bank)
		{
			RuntimeManager.UnloadBank(bank);
		});
		foreach (RoomRef fortressRoomRef in ScriptableObjectSingleton<FortressDataManager>.instance.fortressRoomRefs)
		{
			if (!fortressRoomRef.IsLoaded(assetCatalog))
			{
				yield return fortressRoomRef.StartLoad(assetCatalog);
			}
		}
		fortressState = new FortressStateInfo();
		BootSetter<FortressStateInfo>.BootAttach(assetCatalog.fortressManagerRef.Asset as GameObject, fortressState);
		yield return fortressState.InitCor(this);
	}

	private void PrepareFirstGameAfterCompletedIntroForTest()
	{
		bootStrapLoadData.targetContext = BootStrapLoadData.TargetContext.GameMap;
		bootStrapLoadData.skipGuide = false;
		bootStrapLoadData.skipStory = false;
		bootStrapLoadData.startUnlockAll = false;
		savedGameState = null;
		guideManager.CompleteIntroForTest();
		NewGameConfigData newGameConfigData = NewGameConfigData.DefaultStartGameConfig(this);
		newGameConfigData.startGameInfo = NewGameConfigData.GenerateLocalPrepareInfo(this);
		newGameConfigManager.currentNewGameConfig = newGameConfigData;
		bootStrapLoadData.newGameConfigData = newGameConfigData;
		savingManager.SaveProfile();
	}

	public void PrepareFortressReturnTransition(BootStrapLoadData loadData)
	{
		fortressReturnTransition?.Cancel("replaced by another load");
		fortressReturnTransition = null;
		if (loadData != null && loadData.HasFortressReturnIntent && loadData.targetContext == BootStrapLoadData.TargetContext.Fortress && loadData.enterFortressType == BootStrapLoadData.EnterFortressType.Default && !loadData.skipFortressAutoPopups && fortressState != null)
		{
			FortressReturnTransitionCover fortressReturnTransitionCover = BootstrapExtra.instance?.fortressReturnTransitionCover;
			if (!(fortressReturnTransitionCover == null))
			{
				fortressReturnTransition = new FortressChallengeReturnTransition(this, loadData, fortressReturnTransitionCover);
				fortressReturnTransition.Arm();
			}
		}
	}

	public IEnumerator AfterLoadCor(BootStrapLoadData bootStrapLoadData)
	{
		if (fortressState == null)
		{
			yield break;
		}
		FortressChallengeReturnTransition returnTransition = fortressReturnTransition;
		bool deferClaimMailPolling = false;
		bool claimMailPollingStarted = false;
		bool claimMailPollingScheduled = false;
		try
		{
			Bootstrap.instance.audioManager.PlaySustained(ScriptableObjectSingleton<AudioData>.instance.bgm_menu_fortress, "BgmTag");
			foreach (FortressNPC validAsset in assetCatalog.fortressNPCCatalog.ValidAssets)
			{
				fortressState.uiManager.mainUIPanel.TrySpawnNPC(validAsset);
			}
			eventManager.enterFortressPanelEvent.TriggerEvent();
			returnTransition?.CancelIfVisibleInterruption("enter fortress presentation");
			yield return new WaitUntil(() => storyPointManager.RunningStoryPoint == null && currentCutSceneState == null && !Bootstrap.instance.bootstrapExtra.gameAsyncOperationManager.IsBlockingOperationRunning);
			returnTransition?.CancelIfVisibleInterruption("post-entry presentation");
			yield return WaitForFortressMainRoomBeforeAutoPopupsCor();
			if (!bootStrapLoadData.skipFortressAutoPopups)
			{
				yield return purchaseManager.ShowPendingRestoredOrderRewardsCor(delegate
				{
					CancelReturnTransition("restored order rewards");
				});
				yield return dailyCheckManager.EnterFortressDailyCheckInCor(delegate
				{
					CancelReturnTransition("daily check-in");
				});
				yield return purchaseManager.PromoteLimitedTimeBundleCor(delegate
				{
					CancelReturnTransition("limited-time bundle");
				});
				yield return purchaseManager.PromoteFortressPurchaseCor(delegate
				{
					CancelReturnTransition("purchase promotion");
				});
			}
			if (GlobalConfigManager.instance.enableNetwork)
			{
				mailManager.StartGetMailsPolling();
				if (returnTransition?.IsPending ?? false)
				{
					deferClaimMailPolling = true;
				}
				else
				{
					StartClaimMailPollingOnce();
				}
			}
			if (bootStrapLoadData.enterFortressType == BootStrapLoadData.EnterFortressType.PvpEnemyPanel)
			{
				fortressState.uiManager.pvpModeMainPanel.SetActiveAndDirty(active: true);
				fortressState.uiManager.pvpModeMainPanel.defenderDetailPanel.Setup((defender: fortressState.applicationState.pvpModeManager.testBattleDefender, editMode: fortressState.applicationState.pvpModeManager.testBattleDefenderEditMode));
				fortressState.uiManager.pvpModeMainPanel.defenderDetailPanel.SetActiveAndDirty(active: true);
			}
			else if (bootStrapLoadData.enterFortressType == BootStrapLoadData.EnterFortressType.CharacterInfoPanel)
			{
				PlayerCharacterState state = bootStrapLoadData.enterFortressCharacter.GetState(this);
				fortressState.uiManager.characterInfoBasePanel.ShowUI(state);
				yield return characterTrialManager.ShowPendingCompletionRewardsCor(state.def);
			}
			if (!bootStrapLoadData.skipFortressAutoPopups && gameStatisticsManager.gameCount >= 3)
			{
				if (fortressState.uiManager.bindAccountLoginHint.NeedBindAccount)
				{
					CancelReturnTransition("account binding");
				}
				fortressState.uiManager.bindAccountLoginHint.ShowIfNeeded();
				yield return new WaitUntil(() => !fortressState.uiManager.bindAccountLoginHint.gameObject.activeSelf);
			}
			if (!bootStrapLoadData.skipFortressAutoPopups)
			{
				yield return dataMigrationManager.TryTriggerMigration(delegate
				{
					CancelReturnTransition("data migration");
				});
			}
			if (!bootStrapLoadData.skipFortressAutoPopups && gameStatisticsManager.gameCount >= 2)
			{
				yield return playerInfoManager.TryTriggerReview(delegate
				{
					CancelReturnTransition("review prompt");
				});
			}
			if (returnTransition?.IsPending ?? false)
			{
				bool flag = returnTransition.TryRestoreTarget(deferClaimMailPolling ? new Action(StartClaimMailPollingOnce) : null);
				claimMailPollingScheduled = flag & deferClaimMailPolling;
			}
			if (deferClaimMailPolling && !claimMailPollingScheduled)
			{
				StartClaimMailPollingOnce();
			}
		}
		finally
		{
			ApplicationStateInfo applicationStateInfo = this;
			if (returnTransition?.IsPending ?? false)
			{
				returnTransition.Cancel("after-load flow ended");
			}
			if (deferClaimMailPolling && !claimMailPollingScheduled)
			{
				StartClaimMailPollingOnce();
			}
			if (applicationStateInfo.fortressReturnTransition == returnTransition)
			{
				applicationStateInfo.fortressReturnTransition = null;
			}
		}
		void CancelReturnTransition(string reason)
		{
			returnTransition?.Cancel(reason);
		}
		void StartClaimMailPollingOnce()
		{
			if (!claimMailPollingStarted)
			{
				claimMailPollingStarted = true;
				if (Bootstrap.instance.CurrentApplicationState == this && fortressState != null)
				{
					mailManager.StartClaimMailAttachmentPolling();
				}
			}
		}
	}

	private IEnumerator WaitForFortressMainRoomBeforeAutoPopupsCor()
	{
		FortressUIManager uiManager = fortressState?.uiManager;
		FortressMainUIPanel mainUIPanel = uiManager?.mainUIPanel;
		MainUISwitchGroup switchGroup = mainUIPanel?.mainUISwitchGroup;
		if (uiManager == null || mainUIPanel == null || switchGroup == null)
		{
			yield break;
		}
		yield return new WaitUntil(delegate
		{
			if (fortressState?.uiManager == null)
			{
				return true;
			}
			if (uiManager.transforming)
			{
				return false;
			}
			if (uiManager.currentPanel != null && uiManager.currentPanel != mainUIPanel)
			{
				return false;
			}
			FortressMainUIPanel.MainPanelLayerSetup currentLayerSetups = switchGroup.currentLayerSetups;
			return currentLayerSetups == null || currentLayerSetups.layer == FortressMainUIPanel.MainPanelLayer.MainRoom;
		});
	}

	public IEnumerator RequestSplitProfileDataAfterFetchPlayerData()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			GameAsyncOperation gameAsyncOperation = recordManager.RequestRecordProfileDataIfNeeded();
			forerunnerManager.RequestForerunnerProfileDataIfNeeded();
			bool cardSkinOffersLoaded = false;
			purchaseManager.CardSkinOffers.RequestNetworkOffersIfNeeded(delegate
			{
				cardSkinOffersLoaded = true;
			});
			giftCodeManager.RequestConfigIfNeeded();
			pleaseReviewPopupManager.RequestConfigIfNeeded();
			if (gameAsyncOperation != null)
			{
				yield return gameAsyncOperation.WaitForEnd();
			}
			yield return new WaitWhile(() => forerunnerManager.forerunnerProfileDataLoading);
			yield return new WaitUntil(() => cardSkinOffersLoaded);
			yield return giftCodeManager.LoadConfigIfNeededCor();
			yield return pleaseReviewPopupManager.LoadConfigIfNeededCor();
			dataMigrationManager.RequestStatusIfNeeded();
		}
	}

	private void InitSubManager<T>(ref T manager) where T : GlobalGameManager, new()
	{
		if (manager == null)
		{
			manager = new T();
		}
		try
		{
			manager.Init(this);
		}
		catch (Exception e)
		{
			TraceLog.Exception(e);
		}
		subManagers.Add(manager);
	}

	public void FrameUpdate()
	{
		if (!Inited)
		{
			return;
		}
		if (fortressState != null && fortressState.inited)
		{
			fortressState.FrameUpdate();
		}
		if (runningGameState != null && runningGameState.inited)
		{
			runningGameState.FrameUpdate();
		}
		foreach (GlobalGameManager subManager in subManagers)
		{
			if (subManager is IFrameUpdate frameUpdate)
			{
				frameUpdate.FrameUpdate();
			}
		}
	}

	public void Dispose()
	{
		fortressReturnTransition?.Cancel("application state disposed");
		fortressReturnTransition = null;
		exceptionTraceManager?.Dispose();
		runningGameState?.Dispose();
		fortressState?.Dispose();
		inited = false;
	}

	public WrapperEnumerator StartCoroutine(Func<IEnumerator> enumerator)
	{
		return WrapperEnumerator.StartCoroutine(enumerator, applicationUIManager);
	}

	public void RecordLastGameEndReason(LastGameEndReason reason)
	{
		if (lastGameEndReason != reason)
		{
			lastGameEndReason = reason;
			savingManager?.SetProfileDirtyToSave();
		}
	}

	public bool ShouldGiveStarterBlessing()
	{
		ClavisDef clavisDef = assetCatalog.FullIdToCTAsset<ClavisDef>("#CLA#000029#");
		if (clavisDef == null)
		{
			TraceLog.Error("找不到难度选择解锁钥匙: #CLA#000029#");
			return false;
		}
		return !clavisManager.IsUnlocked(clavisDef);
	}

	public void TestUnlockAll()
	{
		discoverManager.DiscoverAll();
		foreach (MatterInfo matterInfo in matterManager.matterInfos)
		{
			matterManager.AddMatter(matterInfo.matterDef, 9999, "测试");
		}
		globalSkillManager.TestUnlockAll();
		globalUnlockManager.UnlockAll();
		clavisManager.TestUnlockAll();
		savingManager.SetProfileDirtyToSave();
		difficultyManager.finishedMaxDifficulty = (DifficultyLevel)999;
		foreach (IdolState item in idolManager.idolStates.OrEmptyIfNull())
		{
			item.mainlineDifficulty = (DifficultyLevel)999;
		}
		startGameBuffManager.TestUnlockAll();
		pvpModeManager.TestUnlockAll();
		skillGemManager.TestUnlockAll();
		gameStatisticsManager.gameCount = 2;
		List<ProfileSkill> list = new List<ProfileSkill>();
		foreach (SkillDef testInitSkill in ScriptableObjectSingleton<FortressDataManager>.instance.testInitSkills)
		{
			list.Add(globalSkillManager.AddSkillDef(testInitSkill, 0, 0));
		}
		foreach (PlayerCharacterState characterState in characterManager.characterStates)
		{
			ProfileSkillPreset skillPreset = characterState.skillPreset;
			for (int i = 0; i < list.Count && i < skillPreset.configSkillSlots.Count; i++)
			{
				skillPreset.configSkillSlots[i].SetSkill(list[i]);
			}
		}
	}
}
