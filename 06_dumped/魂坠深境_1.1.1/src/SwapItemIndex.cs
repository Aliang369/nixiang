using System.Collections.Generic;

public class SwapItemIndex : Effect
{
	public ItemTargetSelector first;

	public ItemTargetSelector second;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = first?.GetSingleItem(eve);
		Item item2 = second?.GetSingleItem(eve);
		if (item == null || item2 == null)
		{
			return NodeCastResult.Failure;
		}
		transformVFXEffect?.Play(eve, eve.CasterPivot, item);
		transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
		Effect.EnqueueAction(eve, new SwapSlotItemAction(item, item2, eve.CasterItem));
		hitEffect?.Play(eve, item);
		hitEffect?.Play(eve, item2);
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { first, second };
	}
}
