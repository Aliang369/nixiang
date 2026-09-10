using UnityEngine;

public class BonusLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.BonusGame;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 25f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (!gameState.applicationState.globalVariableManager.GetVariableFlag(ApplicationGlobalVariableManager.EnableBonusLocationGenerate))
		{
			return true;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DontGenerateBonusLocation))
		{
			return true;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => l.type == PathLocation.LocationType.BonusGame))
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 4);
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.BonusGame).Count >= num)
		{
			return true;
		}
		return false;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		return new EncounterSetupInfo
		{
			level = gameState.contractState.currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.bonusLocationEvent,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
