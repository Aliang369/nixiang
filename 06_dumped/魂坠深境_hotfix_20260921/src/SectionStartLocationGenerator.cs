public class SectionStartLocationGenerator : LocationGenerator
{
	public override PathLocation.LocationType LocationType => PathLocation.LocationType.SectionStart;

	public override bool CheckRejectSpawn(GameStateInfo gameState, DungeonMap map, PathLocation insertLoc)
	{
		return true;
	}

	public override EncounterSetupInfo GenerateRoomSetupInfo(GameStateInfo gameState, DungeonMap map, PathLocation loc)
	{
		Level currentLevel = gameState.contractState.currentLevel;
		return new EncounterSetupInfo
		{
			fixedEventTemplate = GetSectionStartEventTemplate(gameState),
			level = currentLevel,
			roomInfoData = LocationGenerator.GetLocationRoomInMap(gameState, map, loc),
			enemyLevel = loc.enemyDifficultyLevel
		};
	}

	private static WorldEvent GetSectionStartEventTemplate(GameStateInfo gameState)
	{
		int stage = gameState.contractState.stage;
		if (stage == 2 || stage == 4 || stage == 6)
		{
			GameContractState contractState = gameState.contractState;
			if ((contractState == null || contractState.selectedContract?.contractDef?.contractRule?.CanGenerateRiftWorldEvent(gameState) != false) && ScriptableObjectSingleton<DungeonGenerationManager>.instance.riftWorldEventTemplate != null)
			{
				return ScriptableObjectSingleton<DungeonGenerationManager>.instance.riftWorldEventTemplate;
			}
		}
		return ScriptableObjectSingleton<DungeonGenerationManager>.instance.healSpringEventTemplate;
	}
}
