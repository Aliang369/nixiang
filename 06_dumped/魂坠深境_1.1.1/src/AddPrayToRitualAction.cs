public class AddPrayToRitualAction : ExecuteAction
{
	public Pray pray;

	public AddPrayToRitualAction(Pray pray)
	{
		this.pray = pray;
	}

	protected override void Update()
	{
		gameState.ritualManager.currentPrays.Add(pray);
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterAddPrayToRitual(pray);
		});
		if (gameState.ritualManager.currentPrays.Count >= gameState.ritualManager.prayLimit)
		{
			EnqueueAction(new TriggerRitualAction());
		}
	}
}
