public class ExitCurrentLocationToMapAction : ExecuteAction
{
	private bool currentFinished;

	public ExitCurrentLocationToMapAction(bool currentFinished)
	{
		this.currentFinished = currentFinished;
	}

	protected override void Update()
	{
		EnqueueAction(new ExitRoomTransitAnimAction());
		EnqueueAction(new ExitLocationImplAction(gameState.dungeonMapManager.currentLocation, currentFinished));
		EnqueueAction(new EnterMapTransitAnimAction(scrollMap: false));
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAfterExitLocationToMap(gameState.dungeonMapManager.currentLocation);
		}));
		EnqueueAction(new SaveRunningGameAction());
	}
}
