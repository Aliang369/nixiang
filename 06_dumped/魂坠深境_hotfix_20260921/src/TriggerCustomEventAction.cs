public class TriggerCustomEventAction : ExecuteAction
{
	public string eventName;

	private readonly object[] args;

	private readonly object source;

	public TriggerCustomEventAction(string eventName, object source)
	{
		this.eventName = eventName;
		this.source = source;
	}

	protected override void Update()
	{
		if (eventName == null)
		{
			FinishAction();
			return;
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterTriggerCustomEvent(eventName, source);
		});
		FinishAction();
	}
}
