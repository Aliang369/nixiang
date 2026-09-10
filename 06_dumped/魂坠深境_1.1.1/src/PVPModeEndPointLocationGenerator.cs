public class PVPModeEndPointLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.PVPEndPoint;

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return true;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		return new EncounterSetupInfo
		{
			fixedEventTemplate = ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpEndPointWorldEvent,
			level = gameState.contractState.currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
