using System.Collections.Generic;

public class ConvertItem : Effect
{
	public ItemTargetSelector convertTemplate;

	public ItemTargetSelector targetItemSelector;

	public bool showChangedItems;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetItemSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		Item item = convertTemplate?.GetSingleItem(eve);
		if (item == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item2 in list)
		{
			if (item2 != null)
			{
				transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
				Effect.EnqueueAction(eve, new ConvertItemAction(item2, item));
				hitEffect?.Play(eve, item2);
			}
		}
		if (showChangedItems)
		{
			Effect.EnqueueAction(eve, new NotifyItemModificationsAction(list));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { convertTemplate, targetItemSelector };
	}
}
