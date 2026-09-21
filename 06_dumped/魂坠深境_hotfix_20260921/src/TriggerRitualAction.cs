using System.Collections.Generic;

public class TriggerRitualAction : ExecuteAction
{
	protected override void Update()
	{
		List<Pray> currentPrays = gameState.ritualManager.currentPrays;
		for (int i = 0; i < currentPrays.Count; i++)
		{
			EnqueueAction(new TriggerPrayEffectAction(currentPrays[i]));
			EnqueueAction(new RemovePrayFromRitualAction(currentPrays[i]));
		}
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterRitualTrigger();
		});
	}
}
