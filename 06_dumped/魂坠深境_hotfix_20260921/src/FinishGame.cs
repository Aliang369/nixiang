public class FinishGame : Effect
{
	public CheckNode success;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		CheckNode checkNode = success;
		EndGameAction.EndGameType endGameType = ((checkNode == null || checkNode.Check(eve) != true) ? EndGameAction.EndGameType.Death : EndGameAction.EndGameType.Win);
		Effect.EnqueueAction(eve, new EndGameAction(endGameType));
		return NodeCastResult.Success;
	}
}
