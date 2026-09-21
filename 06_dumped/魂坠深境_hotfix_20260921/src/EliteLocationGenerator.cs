using UnityEngine;

public class EliteLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.EliteFight;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		int num = 40;
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.GenerateMoreFightLocation))
		{
			num = 60;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => LocationGenerator.IsFightLocation(l.type)))
		{
			num /= 2;
		}
		return num;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 2)
		{
			return true;
		}
		int count = map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.EliteFight).Count;
		int num = Mathf.Max(1, map.locations.Count / 2);
		if (count >= num)
		{
			return true;
		}
		if (LocationGenerator.GetContinuousConnectCount(map, insertLoc, (PathLocation l) => LocationGenerator.IsFightLocation(l.type)) >= 2)
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
			enemySpawnType = EnemyEncounterType.Elite,
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
