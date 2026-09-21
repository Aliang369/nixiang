public class SwitchToLocationFromRoomAction : ExecuteAction
{
	public PathLocation targetLocation;

	public SwitchToLocationFromRoomAction(PathLocation targetLocation)
	{
		this.targetLocation = targetLocation;
	}

	protected override void Update()
	{
		EnqueueAction(new ExitRoomTransitAnimAction());
		EnqueueAction(new ExitLocationImplAction(gameState.dungeonMapManager.currentLocation, finished: true));
		EnqueueAction(new EnterLocationImplAction(targetLocation));
		EnqueueAction(new EnterRoomTransitAnimAction());
	}
}
