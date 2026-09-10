public class NotifyWorldEventAction : ExecuteAction
{
	public WorldEvent worldEvent;

	public NotifyWorldEventAction(WorldEvent worldEvent)
	{
		this.worldEvent = worldEvent;
	}

	protected override void Update()
	{
		if (!(worldEvent == null))
		{
			worldEvent.NotifyEvent();
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerStartWorldEvent(worldEvent);
			});
		}
	}
}
