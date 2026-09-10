using UnityEngine;

public class CrystalUpgradeLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.CrystalUpgradeRoom;

	public override float GetSpawnGenerateWeight(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return 30f;
	}

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		if (insertLoc.globalProgress < 2)
		{
			return true;
		}
		int num = Mathf.Max(1, map.locations.Count / 15);
		if (map.locations.FindAll((PathLocation l) => l.type == PathLocation.LocationType.CrystalUpgradeRoom).Count >= num)
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
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.crystalUpgradeEventTemplate,
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
