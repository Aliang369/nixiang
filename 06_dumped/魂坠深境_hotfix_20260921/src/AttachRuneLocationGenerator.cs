using UnityEngine;

public class AttachRuneLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.AttachRune;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 50f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DontGenerateBonusLocation))
		{
			return true;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => l.type == PathLocation.LocationType.AttachRune))
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 3);
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.AttachRune).Count >= num)
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
			fixedEnemyInfoData = null,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.gemBonusEvent,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
