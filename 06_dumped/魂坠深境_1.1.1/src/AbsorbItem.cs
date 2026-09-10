using System.Collections.Generic;

public class AbsorbItem : Effect
{
	public ItemTargetSelector absordItemSelector;

	public bool addCounter;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = absordItemSelector.GetTargetItems(eve);
		if (targetItems == null || targetItems.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in targetItems)
		{
			if (item != null)
			{
				transformVFXEffect?.Play(eve, item, eve.CasterPivot);
				hitEffect?.Play(eve, eve.CasterItem);
				if (addCounter)
				{
					Effect.EnqueueAction(eve, new ChangeCounterAction(item.counter, eve.CasterItem, item));
				}
				Effect.EnqueueAction(eve, new ExileItemAction(item, eve.CasterItem));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { absordItemSelector };
	}
}
