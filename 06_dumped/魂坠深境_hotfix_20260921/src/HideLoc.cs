public class HideLoc : DraftLocationModifier
{
	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		location.hideLoc = true;
	}
}
