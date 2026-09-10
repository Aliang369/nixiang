public class SetLocationTypeAndRegenerateRoom : DraftLocationModifier
{
	public PathLocation.LocationType type;

	public PathLocation.LocationVariantType variantType;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		location.type = type;
		location.variantType = variantType;
		location.attachEncounterSetupInfo = ScriptableObjectSingleton<DungeonGenerationManager>.instance.GetLocationGenerator(location.type).GenerateRoomSetupInfo(gs, map, location);
	}
}
