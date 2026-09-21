public class ForerunnerLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.ForerunnerLegacy;

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return true;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		Level currentLevel = gameState.contractState.currentLevel;
		if (loc.forerunnerLegacy == null)
		{
			loc.forerunnerLegacy = ((currentLevel.forerunnerFallbackSkill != null) ? gameState.applicationState.forerunnerManager.GenerateLevelFallbackForerunnerLegacy(currentLevel.forerunnerFallbackSkill, gameState.contractState.stage, map.sourceDraftMap, currentLevel.forerunnerFallbackNote) : gameState.applicationState.forerunnerManager.GenerateDefaultForerunnerLegacy());
		}
		return new EncounterSetupInfo
		{
			fixedEventTemplate = (loc.forerunnerLegacy.shareLegacy ? ScriptableObjectSingleton<DungeonGenerationManager>.instance.shareForerunnerWorldEvent : ScriptableObjectSingleton<DungeonGenerationManager>.instance.forerunnerWorldEvent),
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}
}
