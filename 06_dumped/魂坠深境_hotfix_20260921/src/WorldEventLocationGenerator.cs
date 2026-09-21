using UnityEngine;

public class WorldEventLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.Event;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 25f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 3)
		{
			return true;
		}
		if (LocationGenerator.IsNearTo(map, insertLoc, (PathLocation l) => l.type == PathLocation.LocationType.Event))
		{
			return true;
		}
		int count = map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.Event).Count;
		int num = Mathf.Max(1, map.locations.Count / 4);
		if (count >= num)
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
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = null,
			spawnRandomWorldEvent = true,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
