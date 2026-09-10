internal class SetEndTurn : Effect
{
	public EndTurnAction.Type type;

	public bool outControlPhaseEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		EndTurnAction action = new EndTurnAction(type, outControlPhaseEffect);
		Effect.EnqueueAction(eve, action);
		return NodeCastResult.Success;
	}
}
