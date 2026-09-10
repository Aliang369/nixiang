public class FixDescription : DraftLocationModifier
{
	[LocalizeText("地图特殊地点名字")]
	public string name;

	[LocalizeText("地图特殊地点描述")]
	public string description;

	public override void ModifyLocation(PathLocation location, DraftPivot pivot, DungeonMap map, GameStateInfo gs)
	{
		location.overrideName = name;
		location.overrideDescription = description;
	}
}
