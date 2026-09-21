public sealed class EvilRibbonSwapEffect : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		Item targetItem = eve.TargetItem;
		if (casterItem == null || targetItem == null || casterItem.owner == null || casterItem.owner != targetItem.owner)
		{
			return NodeCastResult.Success;
		}
		Effect.EnqueueAction(eve, new ChangeCoolDownAction(targetItem, casterItem, -1));
		Effect.EnqueueAction(eve, new SwapSlotItemAction(casterItem, targetItem, casterItem));
		return NodeCastResult.Success;
	}
}
