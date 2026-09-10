public class ExitLocationImplAction : ExecuteAction
{
	private readonly PathLocation location;

	private readonly bool finished;

	public ExitLocationImplAction(PathLocation location, bool finished)
	{
		this.location = location;
		this.finished = finished;
	}

	protected override void Update()
	{
		if (location != null)
		{
			location.finished = finished;
			location.visited = true;
			gameState.difficultyState.reachedLocations.Add(location);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerExitLocation(location);
			});
			if (gameState.currentEncounter != null)
			{
				gameState.actionManager.EnqueueAction(new ClearCurrentEncounterAction());
			}
			ThinkingAnalyticsExtends.TrackExitLocation(gameState, location);
		}
	}
}
