public class HalvePlayerMaxHealth : Effect
{
	public bool baseAdd;

	public bool hideNFX = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve?.gameState?.playerState?.bft;
		if (battleFieldTransform == null || (int)battleFieldTransform.maxHealth <= 1)
		{
			return NodeCastResult.Failure;
		}
		int num = -((int)battleFieldTransform.maxHealth / 2);
		if (num == 0)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new ChangeBftMaxHealthAction(battleFieldTransform, num, baseAdd, hideNFX));
		return NodeCastResult.Success;
	}
}
