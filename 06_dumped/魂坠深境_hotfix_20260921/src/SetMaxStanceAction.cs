public class SetMaxStanceAction : GameAction
{
	public int stanceMaxValue;

	public SetMaxStanceAction(int stanceMaxValue)
	{
		this.stanceMaxValue = stanceMaxValue;
	}

	protected override void Update()
	{
		gameState.stanceManager.stanceMaxValue = stanceMaxValue;
		FinishAction();
	}
}
