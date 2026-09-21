using System.Collections;
using System.Collections.Generic;
using GameRandom;
using UnityEngine.ResourceManagement.AsyncOperations;

public class GenerateLevelMapAction : AbstractEnumeratorAction
{
	public Level level;

	public DraftMapRef draftMapRef;

	public FixedContractRouteDef.StageRoute fixedStageRoute;

	public GenerateLevelMapAction(bool bossSection, Level level, DraftMapRef draftMapRef = null, FixedContractRouteDef.StageRoute fixedStageRoute = null)
	{
		this.level = level;
		this.draftMapRef = draftMapRef;
		this.fixedStageRoute = fixedStageRoute;
	}

	protected override IEnumerator Run()
	{
		if (level == null)
		{
			yield break;
		}
		gameState.gameEventManager.TriggerBeforeMapGeneration();
		List<DraftMapRef> generatableDraftMaps = level.GetGeneratableDraftMaps(gameState);
		DraftMapRef selectedRef = draftMapRef ?? generatableDraftMaps.SelectRandom(gameState.randomManager.NextDouble);
		if (selectedRef == null)
		{
			TraceLog.Error("GenerateLevelMapAction: No valid DraftMapRef found in pool");
			yield break;
		}
		AssetCatalog catalog = gameState.applicationState.assetCatalog;
		AsyncOperationHandle asyncOperationHandle = selectedRef.StartLoad(catalog);
		if (asyncOperationHandle.IsValid() && !asyncOperationHandle.IsDone)
		{
			yield return asyncOperationHandle;
		}
		DraftMapInfoData assetPreData = selectedRef.GetAssetPreData(catalog);
		DraftDungeonMapGenerator draftDungeonMapGenerator = new DraftDungeonMapGenerator(new DraftDungeonMapGenerator.DraftMapGenerationConfig
		{
			gameState = gameState,
			baseGlobalProgress = gameState.dungeonMapManager.currentGlobalProgress,
			baseStageProgress = 0,
			mapMaxDifficulty = gameState.difficultyState.StageTopDifficulty,
			mapMinDifficulty = gameState.difficultyState.StageBaseDifficulty,
			stage = gameState.contractState.stage,
			draftMap = assetPreData,
			level = level,
			fixedStageRoute = fixedStageRoute
		});
		gameState.dungeonMapManager.currentLevel = level;
		gameState.dungeonMapManager.SetDungeonMap(draftDungeonMapGenerator.GenerateMap());
	}
}
