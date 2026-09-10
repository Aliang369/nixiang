public class RemovePrayFromRitualAction : ExecuteAction
{
	public Pray pray;

	public RemovePrayFromRitualAction(Pray pray)
	{
		this.pray = pray;
	}

	protected override void Update()
	{
		gameState.ritualManager.currentPrays.Remove(pray);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterRemovePrayFormRitual(pray);
		});
	}
}
