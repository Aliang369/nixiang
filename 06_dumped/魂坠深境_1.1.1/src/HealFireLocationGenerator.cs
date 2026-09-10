using UnityEngine;

public class HealFireLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.HealFire;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 10f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 3)
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 15);
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.HealFire).Count >= num)
		{
			return true;
		}
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DoNotGenerateRandomHealSpring))
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
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.healSpringEventTemplate,
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
