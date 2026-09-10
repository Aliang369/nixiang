public class EndCurrentEncounterAction : ExecuteAction
{
	private readonly bool finish;

	public EndCurrentEncounterAction(bool finish)
	{
		this.finish = finish;
	}

	protected override void Update()
	{
		EnqueueAction(new DestroyEnemyObjectAction(gameState.battleState.enemy));
		EnqueueAction(new EnumeratorAction(() => gameState.currentEncounter.EndEncounterCor(finish)));
	}
}
