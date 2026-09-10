using System.Collections.Generic;

public class RandomSwapItems : Effect
{
	public ItemTargetSelector itemsSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = itemsSelector.GetTargetItems(eve);
		if (targetItems == null || targetItems.Count < 2 || targetItems.Count % 2 == 1)
		{
			return NodeCastResult.Failure;
		}
		for (int i = 0; i < targetItems.Count; i += 2)
		{
			Item item = targetItems[i];
			Item item2 = targetItems[(i + 1) % targetItems.Count];
			transformVFXEffect?.Play(eve, eve.CasterPivot, item);
			transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
			Effect.EnqueueAction(eve, new SwapSlotItemAction(item, item2, eve.CasterItem));
			hitEffect?.Play(eve, item);
			hitEffect?.Play(eve, item2);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemsSelector };
	}
}
