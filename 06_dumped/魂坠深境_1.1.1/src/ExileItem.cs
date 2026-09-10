using System.Collections.Generic;

public class ExileItem : Effect
{
	public ItemTargetSelector targetSelector;

	public HitNFXConfig hitEffect;

	public TransformNFXConfig transformVFXEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in targetItems)
		{
			if (item != null)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item);
				Effect.EnqueueAction(eve, new ExileItemAction(item, eve.CasterItem, hitEffect));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
