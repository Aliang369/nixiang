public class ClearCurrentEncounterAction : GameAction
{
	protected override void Update()
	{
		if (gameState.currentEncounter != null)
		{
			gameState.currentEncounter.Clear();
			gameState.currentEncounter = null;
			FinishAction();
		}
	}
}
