using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class GameStateInfo : GlobalGameManager
{
	private const string StarterBlessingInfluenceId = "#INF#000359#";

	[SaveField(null, null)]
	public NewGameConfigData newGameConfigData;

	[SaveField(null, null)]
	public bool secondFormalGameFixedRun;

	[SaveField(null, null)]
	public new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	public BattleStateInfo battleState;

	[SaveField(null, null)]
	public GameContractState contractState;

	[SaveField(null, null)]
	public ContractRuntimeStateInfo contractRuntimeStateInfo;

	[SaveField(null, null)]
	public GameDifficultyState difficultyState;

	[SaveField(null, null)]
	public DungeonMapManager dungeonMapManager;

	[SaveField(null, null)]
	public GameRandomManager randomManager;

	[SaveField(null, null)]
	public GameTimeManager gameTimer;

	[SaveField(null, null)]
	public GameGenerationManager generationManager;

	[SaveField(null, null)]
	public InGameGlobalVariableStateInfo globalVariableStateInfo;

	[SaveField(null, null)]
	public MainStateManager mainStateManager;

	[SaveField(null, null)]
	public PivotManager pivotManager;

	[SaveField(null, null)]
	public PlayerGameStateInfo playerState;

	[SaveField(null, null)]
	public InGameRecorderManager recordManager;

	[SaveField(null, null)]
	public ClassMatterManager classMatterManager;

	[SaveField(null, null)]
	public List<StartGameBuff> startGameBuffs;

	[SaveField(null, null)]
	public bool inited;

	[SaveField(null, null)]
	public bool showConfirm;

	[SaveField(null, null)]
	public bool showSkillReplaceConfirm;

	[SaveField(null, null)]
	public RitualManager ritualManager;

	[SaveField(null, null)]
	public TrigramsManager trigramsManager;

	[SaveField(null, null)]
	public StanceManager stanceManager;

	[SaveField(null, null)]
	public AntiCheatSaveState antiCheatState = new AntiCheatSaveState();

	public ActionManager actionManager;

	public GameEventManager gameEventManager;

	public BootStrapLoadData startBootStrapData;

	public EncounterInfo currentEncounter;

	public GameObject sceneGo;

	public BattleFieldTransformDisplay playerBftDisplay;

	private readonly List<InGameManager> subManagers = new List<InGameManager>();

	public GameUIManager UIManager;

	public bool gameEnded;

	public bool loaded;

	private int playerInputLockCount;

	private static bool StartGameBuffEntryEnabled => false;

	public bool IsIntroGame
	{
		get
		{
			NewGameConfigData obj = newGameConfigData;
			if (obj == null)
			{
				BootStrapLoadData bootStrapLoadData = startBootStrapData;
				if (bootStrapLoadData == null)
				{
					return false;
				}
				return bootStrapLoadData.toIntro == true;
			}
			return obj.ToIntro;
		}
	}

	public bool IsPvpEnemyTestGame => contractState.selectedContract.contractDef.contractType == ContractDef.ContractType.PVPEnemyTest;

	public bool IsCharacterTrialGame => contractState.selectedContract.contractDef.contractType == ContractDef.ContractType.CharacterTrial;

	private bool IsSandboxBattleGame
	{
		get
		{
			if (!IsPvpEnemyTestGame)
			{
				return IsCharacterTrialGame;
			}
			return true;
		}
	}

	public string GameGUID => newGameConfigData.startGameInfo.gameGuid;

	public override bool Inited
	{
		get
		{
			if (inited && SceneInfoSingleton.instance != null)
			{
				return subManagers.TrueForAll((InGameManager m) => m.Inited);
			}
			return false;
		}
	}

	public bool PlayerControlAble
	{
		get
		{
			if (playerInputLockCount == 0 && !applicationState.guideManager.blockPlayerInput)
			{
				if (!battleState.isInBattle || (battleState.turnPhase != TurnPhase.PlayerControlPhase && battleState.turnPhase != TurnPhase.PlayerTurnEndPhase))
				{
					return !battleState.isInBattle;
				}
				return true;
			}
			return false;
		}
	}

	public void AcquirePlayerInputLock()
	{
		playerInputLockCount++;
	}

	public void ReleasePlayerInputLock()
	{
		if (playerInputLockCount == 0)
		{
			throw new InvalidOperationException("Player input lock released without a matching acquire.");
		}
		playerInputLockCount--;
	}

	public void ResetBattleRuntimeInputState()
	{
		playerInputLockCount = 0;
		playerState.ResetPlayerSpellRuntimeState();
		UIManager.playerItemArea.controlInfo.ResetBattleRuntimeState();
		actionManager.ResetLocks();
	}

	private bool ShouldShowStartGameBuffConfigPanel()
	{
		if (StartGameBuffEntryEnabled && applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableStartGameBuffSystem))
		{
			return applicationState.startGameBuffManager.WithAnyActivatableStartGameBuff();
		}
		return false;
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		InitSubManager(ref randomManager);
		InitSubManager(ref actionManager);
		InitSubManager(ref mainStateManager);
		InitSubManager(ref gameEventManager);
		InitSubManager(ref playerState);
		InitSubManager(ref battleState);
		InitSubManager(ref globalVariableStateInfo);
		InitSubManager(ref difficultyState);
		InitSubManager(ref contractState);
		InitSubManager(ref contractRuntimeStateInfo);
		InitSubManager(ref dungeonMapManager);
		InitSubManager(ref recordManager);
		InitSubManager(ref classMatterManager);
		InitSubManager(ref gameTimer);
		InitSubManager(ref trigramsManager);
		InitSubManager(ref stanceManager);
		InitSubManager(ref ritualManager);
		InitSubManager(ref generationManager);
		InitSubManager(ref pivotManager);
		UIManager.Init();
		currentEncounter = null;
		sceneGo.SetActive(value: true);
		inited = true;
		loaded = false;
	}

	private void InitSubManager<T>(ref T manager) where T : InGameManager, new()
	{
		if (manager == null)
		{
			manager = new T();
		}
		manager.Init(this);
		subManagers.Add(manager);
	}

	public override void Dispose()
	{
		inited = false;
		UIManager.Dispose();
		playerState.Dispose();
		gameEventManager.Dispose();
		if (contractState.currentLevel != null)
		{
			contractState.currentLevel.Unload(applicationState.assetCatalog);
		}
	}

	public void FrameUpdate()
	{
		if (!inited)
		{
			return;
		}
		actionManager.Update();
		battleState.enemy?.FrameUpdate();
		try
		{
			UIManager.FrameUpdate();
		}
		catch (Exception e)
		{
			TraceLog.Exception(e, "UIFrameUpdate异常", ThinkingAnalyticsExtends.ExceptionType.UIException);
		}
	}

	public void EndGame(bool success, bool finished, LastGameEndReason? endReasonOverride = null)
	{
		if (gameEnded)
		{
			return;
		}
		gameEnded = true;
		if (base.gameState.IsCharacterTrialGame)
		{
			LoadToFortressAfterCharacterTrial(success);
			return;
		}
		if (base.gameState.IsPvpEnemyTestGame)
		{
			Bootstrap.instance.Load(BootStrapLoadData.LoadToFortress(Bootstrap.instance.CurrentAccountID, BootStrapLoadData.EnterFortressType.PvpEnemyPanel));
			return;
		}
		actionManager.CleanActionQueue();
		bool isAfterIntroFirstGame = applicationState.guideManager.IsAfterIntroFirstGame;
		gameEventManager.TriggerBeforeGameEnd(success, isAfterIntroFirstGame);
		applicationState.achievementManager.UploadBatchedFinishedAchievements();
		battleState.StopBattleState();
		gameTimer.Stop();
		GameRecord record = recordManager.GenerateGameRecord(success, finished, isAfterIntroFirstGame);
		applicationState.RecordLastGameEndReason((LastGameEndReason)(((int?)endReasonOverride) ?? (success ? 1 : (finished ? 2 : 3))));
		record.TriggerResolve(applicationState, delegate
		{
			AfterGameRecordResolve(record);
		});
		void AfterGameRecordResolve(GameRecord gameRecord)
		{
			UIManager.gameOverPanel.Setup(gameRecord);
			applicationState.eventManager.afterGameOverEvent.TriggerEvent(gameRecord);
			ThinkingAnalyticsExtends.TrackGameOver(this, gameRecord);
			actionManager.EnqueueAction(delegate
			{
				gameEventManager.TriggerAfterGameEnd(gameRecord.success);
				applicationState.achievementManager.UploadBatchedFinishedAchievements();
			});
			applicationState.recordManager.SaveGameRecord(gameRecord);
			applicationState.savedGameState = null;
			applicationState.newGameConfigManager.ResetAfterLimitedDeploymentRun(newGameConfigData);
			applicationState.savingManager.SaveProfile();
		}
	}

	public IEnumerator StartGameCor()
	{
		RenderSettings.fogStartDistance = 7f;
		RenderSettings.fogEndDistance = 81f;
		UIHelper.EnableAllButton();
		((Component)(object)UIManager.mainCanvas).gameObject.SetActive(value: true);
		UIManager.uiVFXRoot.gameObject.SetActive(value: true);
		playerBftDisplay.gameObject.SetActive(value: true);
		playerState.bft.attachDisplay.gameObject.SetActive(value: true);
		if (startBootStrapData.IsNewGame)
		{
			secondFormalGameFixedRun = false;
			showConfirm = true;
			showSkillReplaceConfirm = true;
			globalVariableStateInfo.Reset();
			contractRuntimeStateInfo.Reset();
			ProtectedValueGuard.Reset();
			startGameBuffs = new List<StartGameBuff>();
			newGameConfigData = startBootStrapData.newGameConfigData;
			if (newGameConfigData.contract != null)
			{
				contractState.selectedContract = newGameConfigData.contract;
			}
			else
			{
				contractState.selectedContract = applicationState.contractManager.defaultGameContract;
			}
			secondFormalGameFixedRun = applicationState.guideManager.ShouldStartSecondFormalGameFixedRun(newGameConfigData);
			contractState.stage = 0;
			gameEventManager.TriggerBeforeGameGeneration();
			PlayerCharacterState orCreateCharacterState = applicationState.characterManager.GetOrCreateCharacterState(newGameConfigData.currentCharacter);
			actionManager.EnqueueAction(new SetupPlayerCharacterAction(orCreateCharacterState, startBootStrapData));
			yield return new WaitUntil(() => actionManager.isEmpty);
			if (newGameConfigData.ToIntro)
			{
				applicationState.guideManager.RunIntroModule(this);
			}
			else
			{
				EnqueueStarterBlessingIfNeeded();
				bool toBattle = startBootStrapData.targetContext == BootStrapLoadData.TargetContext.GameBattle;
				DraftMapRef map = null;
				if (applicationState.guideManager.IsAfterIntroFirstGame)
				{
					map = ScriptableObjectSingleton<GuideData>.instance.afterIntroFirstDraftMapRef;
				}
				else if (secondFormalGameFixedRun)
				{
					map = ScriptableObjectSingleton<GuideData>.instance.secondFormalGameFirstStageDraftMapRef;
				}
				if (contractState.selectedContract.contractDef.contractType == ContractDef.ContractType.PVPOffender)
				{
					map = ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpChallengeMapRef;
				}
				bool flag = ShouldShowStartRootBlessUpgrade();
				actionManager.EnqueueAction(new EnterNewStageAction(1, toWelcome: false, showLoading: false, null, map, !flag));
				actionManager.EnqueueAction(new EnumeratorAction(UIManager.showMapFogPanel.ShowFog));
				yield return new WaitUntil(() => actionManager.isEmpty);
				BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
				yield return new WaitUntil(() => actionManager.isEmpty && !BootstrapExtra.instance.loadingPanel.showing);
				if (toBattle)
				{
					EncounterSetupInfo info = startBootStrapData.BuildInitEncounterSetupInfo(applicationState);
					actionManager.EnqueueAction(new SetupEncounterAction(info));
					actionManager.EnqueueAction(new EnterRoomTransitAnimAction());
				}
				else
				{
					Image image = null;
					if (ShouldShowStartGameBuffConfigPanel())
					{
						UIManager.startGameBuffConfigPanel.taskPanding = true;
						actionManager.EnqueueAction(new ShowStartGameBuffConfigPanelAction());
						yield return UIManager.showMapFogPanel.HideFog();
						yield return new WaitUntil(() => !UIManager.startGameBuffConfigPanel.taskPanding && !UIManager.startGameBuffConfigPanel.gameObject.activeInHierarchy);
						yield return UIManager.showMapFogPanel.ShowFog();
						actionManager.EnqueueAction(new ActionAction(delegate
						{
							image = base.gameState.UIManager.gameVirtualCamera.cameraEffectImagePool.Spawn();
							image.color = Color.black;
						}));
					}
					yield return ShowRootBlessUpgradeIfNeededCor();
					actionManager.EnqueueAction(delegate
					{
						gameEventManager.TriggerBeforeStartGame();
					});
					actionManager.EnqueueAction(new EnterLocationImplAction(base.gameState.dungeonMapManager.currentDungeonMap.initLocation));
					actionManager.EnqueueAction(new EnterRoomTransitAnimAction());
					actionManager.EnqueueAction(new ActionAction(delegate
					{
						base.gameState.gameEventManager.TriggerAfterEnterLocationScreenFade(base.gameState.dungeonMapManager.currentDungeonMap.initLocation);
					}));
					actionManager.EnqueueAction(new ActionAction(delegate
					{
						if (!(image == null))
						{
							base.gameState.UIManager.gameVirtualCamera.cameraEffectImagePool.Despawn(image);
						}
					}));
				}
			}
			actionManager.EnqueueAction(new ActionAction(delegate
			{
				recordManager.enabledJournalRecord = true;
			}));
			yield return new WaitUntil(() => actionManager.isEmpty);
		}
		else
		{
			recordManager.enabledJournalRecord = false;
			AsyncOperationHandle? asyncOperationHandle = dungeonMapManager.currentDungeonMap?.sourceDraftMap?.StartLoad(applicationState.assetCatalog);
			if (asyncOperationHandle.HasValue && asyncOperationHandle.Value.IsValid() && !asyncOperationHandle.Value.IsDone)
			{
				yield return asyncOperationHandle.Value;
			}
			UIManager.dungeonMapPanel.Setup(dungeonMapManager.currentDungeonMap);
			UIManager.dungeonMapPanel_Preview.Setup(dungeonMapManager.currentDungeonMap);
			UIManager.playerBftUIDisplay.Setup(playerState.bft);
			actionManager.EnqueueAction(new LoadLevelAction(contractState.currentLevel));
			bool toBattle = dungeonMapManager.currentLocation != null && !dungeonMapManager.currentLocation.finished;
			if (toBattle)
			{
				actionManager.EnqueueAction(new EnumeratorAction(UIManager.showMapFogPanel.ShowFog));
			}
			yield return new WaitUntil(() => actionManager.isEmpty);
			BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
			Bootstrap.instance.audioManager.PlaySustained(dungeonMapManager.currentLevel.environmentMusicRef, "majorEnvironmentTag");
			Bootstrap.instance.audioManager.PlaySustained(dungeonMapManager.currentLevel.peaceMusicRef, "BgmTag");
			yield return new WaitUntil(() => actionManager.isEmpty && !BootstrapExtra.instance.loadingPanel.showing);
			if (toBattle)
			{
				actionManager.EnqueueAction(new EnterLocationImplAction(dungeonMapManager.currentLocation, saveOnEnter: false));
				actionManager.EnqueueAction(new EnterRoomTransitAnimAction());
			}
			else
			{
				actionManager.EnqueueAction(new ActionAction(delegate
				{
					UIManager.dungeonMapPanel.ShowPanel();
					UIManager.dungeonMapPanel.dungeonMapDisplay.updateLocations = true;
					UIManager.dungeonMapPanel.dungeonMapDisplay.SetDirty();
					UIManager.dungeonMapPanel.dungeonMapDisplay.SetViewRectToLocation(dungeonMapManager.currentLocation);
				}));
			}
			actionManager.EnqueueAction(new ActionAction(delegate
			{
				recordManager.enabledJournalRecord = true;
			}));
		}
		UIManager.SetDirty();
		gameEventManager.TriggerAfterLoadGame();
		Bootstrap.instance.adsManager.LoadAll();
		loaded = true;
	}

	private void EnqueueStarterBlessingIfNeeded()
	{
		if (!IsSandboxBattleGame && applicationState.ShouldGiveStarterBlessing())
		{
			InfluenceDef influenceDef = applicationState.assetCatalog.FullIdToCTAsset<InfluenceDef>("#INF#000359#");
			if (influenceDef == null)
			{
				TraceLog.Error("找不到开局保底祝福: #INF#000359#");
			}
			else if (!playerState.bft.influences.Exists((Influence influence) => influence.def == influenceDef))
			{
				actionManager.EnqueueAction(new AddInfluenceAction(new Influence(influenceDef), playerState.bft));
			}
		}
	}

	private IEnumerator ShowRootBlessUpgradeIfNeededCor()
	{
		if (!ShouldShowStartRootBlessUpgrade())
		{
			yield break;
		}
		WorldEvent startWorldEventOverride = GetStartWorldEventOverride();
		if (startWorldEventOverride != null)
		{
			yield return ShowStartWorldEventOverrideCor(startWorldEventOverride);
			yield break;
		}
		DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance.rootBlessUpgradeConfig;
		RootBlessUpgradeEvent rootBlessEvent = ScriptableObject.CreateInstance<RootBlessUpgradeEvent>();
		rootBlessEvent.SetupConfig(rootBlessUpgradeConfig);
		rootBlessEvent.SetupAndShow(this);
		AfterBossUpgradeEventPanel panel = UIManager.worldEventPanel_AfterBossUpgrade;
		yield return panel.ShowPanel();
		panel.ToBossUpgrade();
		yield return UIManager.showMapFogPanel.HideFog();
		yield return new WaitUntil(() => rootBlessEvent.finished);
		yield return new WaitUntil(() => actionManager.isEmpty);
		yield return UIManager.showMapFogPanel.ShowFog();
		yield return panel.HidePanel();
		panel.Unset();
		if (UIManager.currentEventPanel == panel)
		{
			UIManager.currentEventPanel = null;
		}
		if (dungeonMapManager?.currentLevel != null)
		{
			Bootstrap.instance.audioManager.PlaySustained(dungeonMapManager.currentLevel.peaceMusicRef, "BgmTag");
		}
		UnityEngine.Object.Destroy(rootBlessEvent);
	}

	private IEnumerator ShowStartWorldEventOverrideCor(WorldEvent worldEventTemplate)
	{
		WorldEvent worldEvent = UnityEngine.Object.Instantiate(worldEventTemplate);
		worldEvent.SetupAndShow(this);
		WorldEventPanel panel = worldEvent.GetDisplayWorldEventPanel(this);
		if (panel == null)
		{
			TraceLog.Error("开局覆盖事件没有可用面板: " + worldEventTemplate.name);
			UnityEngine.Object.Destroy(worldEvent);
			yield break;
		}
		yield return panel.ShowPanel();
		if (panel is AfterBossUpgradeEventPanel afterBossUpgradeEventPanel)
		{
			afterBossUpgradeEventPanel.ToBossUpgrade();
		}
		yield return UIManager.showMapFogPanel.HideFog();
		yield return new WaitUntil(() => worldEvent.finished);
		yield return new WaitUntil(() => actionManager.isEmpty);
		yield return UIManager.showMapFogPanel.ShowFog();
		yield return panel.HidePanel();
		panel.Unset();
		if (UIManager.currentEventPanel == panel)
		{
			UIManager.currentEventPanel = null;
		}
		if (dungeonMapManager?.currentLevel != null)
		{
			Bootstrap.instance.audioManager.PlaySustained(dungeonMapManager.currentLevel.peaceMusicRef, "BgmTag");
		}
		UnityEngine.Object.Destroy(worldEvent);
	}

	private WorldEvent GetStartWorldEventOverride()
	{
		return contractState?.selectedContract?.contractDef?.contractRule?.GetStartWorldEventOverride(this);
	}

	private bool ShouldShowStartRootBlessUpgrade()
	{
		if (IsSandboxBattleGame)
		{
			return false;
		}
		if (GetStartWorldEventOverride() != null)
		{
			return true;
		}
		DungeonGenerationManager.RootBlessUpgradeConfig rootBlessUpgradeConfig = ScriptableObjectSingleton<DungeonGenerationManager>.instance?.rootBlessUpgradeConfig;
		if (rootBlessUpgradeConfig == null || !rootBlessUpgradeConfig.CheckStartUnlocked(applicationState))
		{
			return false;
		}
		return true;
	}

	public void LoadToFortressAfterCharacterTrial(bool completed)
	{
		applicationState.savedGameState = null;
		PlayerCharacterDef character = newGameConfigData.encounterSetupConfig.trialCharacterDef;
		if (!completed)
		{
			LoadToFortressCharacterInfo(character);
			return;
		}
		applicationState.characterTrialManager.CompleteTrialAndGrantRewards(character, delegate
		{
			LoadToFortressCharacterInfo(character);
		}, delegate
		{
			LoadToFortressCharacterInfo(character);
		});
	}

	private void LoadToFortressCharacterInfo(PlayerCharacterDef character)
	{
		Bootstrap.instance.Load(BootStrapLoadData.LoadToFortressCharacterInfo(Bootstrap.instance.CurrentAccountID, character));
	}

	public WrapperEnumerator StartCoroutine(Func<IEnumerator> enumerator)
	{
		return WrapperEnumerator.StartCoroutine(enumerator, UIManager);
	}

	public WrapperEnumerator StartCoroutine(Func<WrapperEnumerator.Info, IEnumerator> enumerator, WrapperEnumerator.Info info = null)
	{
		return WrapperEnumerator.StartCoroutine(enumerator, UIManager, info);
	}
}
