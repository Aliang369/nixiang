using GameRandom;

public class EnterNewStageAction : ExecuteAction
{
	private readonly int stage;

	private readonly bool showLoading;

	private bool toWelcome;

	private Level newLevel;

	private readonly DraftMapRef map;

	private readonly bool playPeaceMusic;

	public EnterNewStageAction(int stage, bool toWelcome, bool showLoading, Level level = null, DraftMapRef map = null, bool playPeaceMusic = true)
	{
		this.stage = stage;
		this.toWelcome = toWelcome;
		this.showLoading = showLoading;
		newLevel = level;
		this.map = map;
		this.playPeaceMusic = playPeaceMusic;
	}

	protected override void Update()
	{
		FixedContractRouteDef fixedContractRouteDef = gameState.newGameConfigData?.ResolvedFixedContractRoute;
		if (fixedContractRouteDef != null && fixedContractRouteDef.IsCompletedAfterStage(stage - 1))
		{
			EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Win));
			return;
		}
		FixedContractRouteDef.StageRoute stageRoute = fixedContractRouteDef?.GetStage(stage);
		if (stageRoute != null)
		{
			newLevel = stageRoute.level;
		}
		if (newLevel == null)
		{
			newLevel = Level.GetStageLevels(stage, gameState.applicationState.assetCatalog).SelectRandom(gameState.randomManager.NextDouble);
		}
		DraftMapRef draftMapRef = stageRoute?.map ?? map;
		if (stage == 7)
		{
			Item item = FinalEndingKeyUtility.FindPlayerRepairedFinalEndingKey(gameState);
			if (item != null)
			{
				EnqueueAction(new RemoveItemAction(item, null));
			}
		}
		gameState.contractState.stage = stage;
		gameState.gameEventManager.TriggerBeforeEnterNextStage();
		if (gameState.currentEncounter?.room != null)
		{
			EnqueueAction(new ExitRoomTransitAnimAction());
		}
		if (gameState.dungeonMapManager.currentLocation != null)
		{
			EnqueueAction(new ExitLocationImplAction(gameState.dungeonMapManager.currentLocation, finished: true));
		}
		if (showLoading)
		{
			EnqueueAction(new ActionAction(delegate
			{
				ContractPresentationDef.StagePresentation stagePresentation = gameState.contractState?.selectedContract?.contractDef?.presentation?.GetStagePresentation(newLevel);
				BootstrapExtra.instance.loadingPanel.Show(new LoadingPanel.LoadingShowInfo
				{
					showLoadingTitleText = true,
					loadingLevel = newLevel,
					loadingImage = stagePresentation?.loadingImage,
					loadingTitleText = stagePresentation?.loadingTitle,
					loadingHintText = gameState.applicationState.guideManager.GenerateGuideHint()
				});
			}));
		}
		EnqueueAction(new LoadLevelAction(newLevel));
		if (showLoading)
		{
			EnqueueAction(new WaitAction(2f / base.actionTimeScale));
		}
		EnqueueAction(new ClearDungeonMapAction());
		EnqueueAction(new GenerateLevelMapAction(bossSection: false, newLevel, draftMapRef, stageRoute));
		EnqueueAction(delegate
		{
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			gameState.gameEventManager.TriggerAfterEnterNextStage();
			if (showLoading && !toWelcome)
			{
				BootstrapExtra.instance.loadingPanel.NotifyProgress(1f);
			}
			Bootstrap.instance.audioManager.PlaySustained(newLevel.environmentMusicRef, "majorEnvironmentTag");
			if (playPeaceMusic)
			{
				Bootstrap.instance.audioManager.PlaySustained(newLevel.peaceMusicRef, "BgmTag");
			}
			else
			{
				Bootstrap.instance.audioManager.StopSustained("BgmTag");
			}
			gameState.UIManager.SetDirty();
			if (showLoading && !toWelcome)
			{
				EnqueueAction(new WaitUntilAction(() => !BootstrapExtra.instance.loadingPanel.showing));
			}
			if (toWelcome)
			{
				EnqueueAction(new ActionAction(delegate
				{
					PathLocation initLocation = gameState.dungeonMapManager.currentDungeonMap.initLocation;
					EnqueueAction(new EnterLocationAction(initLocation, showLoading));
				}));
			}
		});
	}
}
