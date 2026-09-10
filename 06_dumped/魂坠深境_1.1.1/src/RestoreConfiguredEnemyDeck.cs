public class RestoreConfiguredEnemyDeck : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve.TargetBft ?? eve.CasterOwner;
		if (battleFieldTransform?.characterAI == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new RestoreEnemyDeckAction(battleFieldTransform, eve.CasterItem ?? eve.Caster));
		return NodeCastResult.Success;
	}
}
