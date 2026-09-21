internal class EnemyEscape : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Effect.EnqueueAction(eve, new EnemyEscapeAction());
		return NodeCastResult.Success;
	}
}
