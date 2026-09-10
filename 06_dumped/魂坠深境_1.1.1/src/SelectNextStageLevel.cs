using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class SelectNextStageLevel : WorldEvent
{
	[NonSerialized]
	private List<DraftMapRef> selectableMaps;

	protected override void Init()
	{
		base.Init();
		selectableMaps = new List<DraftMapRef>();
		FixedContractRouteDef fixedContractRouteDef = gameState.newGameConfigData?.ResolvedFixedContractRoute;
		if ((object)fixedContractRouteDef != null && fixedContractRouteDef.IsCompletedAfterStage(gameState.contractState.stage))
		{
			EnqueueAction(new EndGameAction(EndGameAction.EndGameType.Win));
			return;
		}
		FixedContractRouteDef.StageRoute stageRoute = fixedContractRouteDef?.GetStage(gameState.contractState.stage + 1);
		if (stageRoute != null)
		{
			AddLevelMapChoice(stageRoute.level, stageRoute.map);
			return;
		}
		if (gameState.applicationState.guideManager.IsAfterIntroFirstGame && gameState.contractState.stage == 1)
		{
			Level level = gameState.applicationState.assetCatalog.levelDefCatalog.ValidAssets.First((Level x) => x.stage == gameState.contractState.stage + 1 && gameState.applicationState.globalUnlockManager.IsUnlocked(x));
			DraftMapRef afterIntroSecondDraftMapRef = ScriptableObjectSingleton<GuideData>.instance.afterIntroSecondDraftMapRef;
			AddLevelMapChoice(level, afterIntroSecondDraftMapRef);
			return;
		}
		foreach (Level item in gameState.applicationState.assetCatalog.levelDefCatalog.ValidAssets.Where((Level x) => x.stage == gameState.contractState.stage + 1))
		{
			DraftMapRef map = item.GetGeneratableDraftMaps(gameState).SelectRandom(gameState.randomManager.NextDouble);
			AddLevelMapChoice(item, map);
		}
	}

	private void AddLevelMapChoice(Level level, DraftMapRef map)
	{
		AssetCatalog assetCatalog = gameState.applicationState.assetCatalog;
		DraftMapInfoData draftMapInfoData = map?.GetAssetPreData(assetCatalog);
		bool flag = gameState.applicationState.globalUnlockManager.IsUnlocked(level);
		bool flag2 = level.stage != 7 || FinalEndingKeyUtility.HasPlayerRepairedFinalEndingKey(gameState);
		bool executeAble = flag & flag2;
		if (executeAble)
		{
			selectableMaps.Add(map);
		}
		WorldEventChoice worldEventChoice = new WorldEventChoice();
		worldEventChoice.buttonText = (gameState.contractState?.selectedContract?.contractDef?.presentation?.GetStagePresentation(level))?.nextStageButtonText ?? ScriptableObjectSingleton<TextDataPreset>.instance.term_goto_level_arg0_map_arg1.FormatWith(level.Name, draftMapInfoData?.mapDisplayName);
		worldEventChoice.descriptionText = ((!flag) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_need_clavis_to_unlock : ((!flag2) ? ScriptableObjectSingleton<TextDataPreset>.instance.term_need_repaired_final_ending_key_to_unlock : draftMapInfoData?.mapDescription));
		worldEventChoice.executeAbleChecker = () => executeAble;
		worldEventChoice.executeAction = delegate
		{
			Bootstrap.instance.audioManager.StopAllSustained();
			ThinkingAnalyticsExtends.TrackSelectNextStageLevel(gameState, selectableMaps, map);
			EnqueueAction(new EnterNewStageAction(gameState.contractState.stage + 1, toWelcome: true, showLoading: true, level, map));
		};
		currentDisplayInfo.AddChoice(worldEventChoice);
	}
}
