public class CheckInBossBattle : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.dungeonMapManager == null)
		{
			return null;
		}
		return eve.gameState.dungeonMapManager.currentLocation != null && eve.gameState.dungeonMapManager.currentLocation.type == PathLocation.LocationType.Boss;
	}
}
