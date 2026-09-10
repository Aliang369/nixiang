public class FixWorldEvent : DraftLocationModifier
{
	public WorldEvent worldEvent;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		location.attachEncounterSetupInfo.fixedEventTemplate = worldEvent;
	}
}
