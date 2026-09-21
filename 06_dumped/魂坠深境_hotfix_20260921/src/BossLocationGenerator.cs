public class BossLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.Boss;

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return true;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		Level currentLevel = gameState.contractState.currentLevel;
		return new EncounterSetupInfo
		{
			enemySpawnType = EnemyEncounterType.Boss,
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			fixedEventTemplate = ScriptableObjectSingleton<DungeonGenerationManager>.instance.enterBattleEventTemplate,
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
