public abstract class ExecuteAction : GameAction
{
	public override void Excute(GameStateInfo gameState)
	{
		base.Excute(gameState);
		FinishAction();
	}
}
