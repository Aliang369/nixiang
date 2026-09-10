public class ResetTargetItemCoolDown : Effect
{
	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		transformVFXEffect?.Play(eve, eve.CasterPivot, eve.spellInfo.targetItem);
		Effect.EnqueueAction(eve, new SetCoolDownAction(eve.spellInfo.targetItem, eve.CasterItem, 0));
		hitEffect?.Play(eve, eve.spellInfo.targetItem);
		return NodeCastResult.Success;
	}
}
