public class CheckCurrentLocationPivotIndex : CheckNode
{
	public int index;

	public override bool? Check(GameEvent eve)
	{
		PathLocation currentLocation = eve.gameState.dungeonMapManager.currentLocation;
		if (currentLocation == null)
		{
			return null;
		}
		return currentLocation.draftMapIndex == index;
	}
}
