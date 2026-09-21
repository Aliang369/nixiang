using System.Collections.Generic;

public class SummonCompanion : Effect
{
	public ItemTargetSelector target;

	public GameEvent.EventBFTType bftType;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = target?.GetSingleItem(eve);
		BattleFieldTransform eventBFT = eve.GetEventBFT(bftType);
		if (eventBFT == null || item == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, item);
		Effect.EnqueueAction(eve, new SummonCompanionAction(item, eventBFT));
		hitEffect?.Play(eve, item);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { target };
	}
}
