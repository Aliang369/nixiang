using UnityEngine;

public class UpgradeRoomLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.UpgradeRoom;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 40f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 2)
		{
			return true;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => l.type == PathLocation.LocationType.UpgradeRoom))
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 10);
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateOnlyOneUpgrade))
		{
			num = 1;
		}
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.UpgradeRoom).Count >= num)
		{
			return true;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.NoGenerateUpgradeRoom))
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
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.upgradeRoomEventTemplate,
			level = currentLevel,
			roomInfoData = ScriptableObjectSingleton<DungeonGenerationManager>.instance.upgradeRoomRef.GetAssetPreData(gameState.applicationState.assetCatalog),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
