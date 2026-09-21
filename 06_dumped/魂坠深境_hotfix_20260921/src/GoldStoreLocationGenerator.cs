using UnityEngine;

public class GoldStoreLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.GoldStore;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		float num = 40f;
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MoreStoreRoomGeneration))
		{
			num *= 2f;
		}
		return num;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 4)
		{
			return true;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => l.type == PathLocation.LocationType.GoldStore))
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 10);
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.MoreStoreRoomGeneration))
		{
			num++;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateOnlyOneStore))
		{
			num = 1;
		}
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.GoldStore).Count >= num)
		{
			return true;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoGenerateStore))
		{
			return true;
		}
		return false;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		Level currentLevel = gameState.contractState.currentLevel;
		return new EncounterSetupInfo
		{
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.relicStoreTemplate,
			level = currentLevel,
			roomInfoData = ScriptableObjectSingleton<DungeonGenerationManager>.instance.storeRoomRef.GetAssetPreData(gameState.applicationState.assetCatalog),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
