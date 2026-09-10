using System.Collections.Generic;

public class CombineItem : Effect
{
	public ItemTargetSelector combineTemplate;

	public ItemTargetSelector targetItemSelector;

	public TransformNFXConfig transformVFXEffect;

	public HitNFXConfig hitEffect;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetItemSelector?.GetTargetItems(eve);
		Item item = combineTemplate?.GetSingleItem(eve);
		if (item == null || list == null)
		{
			return NodeCastResult.Failure;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Item item2 = list[i];
			transformVFXEffect?.Play(eve, eve.CasterPivot, item2);
			if (i == 0)
			{
				Effect.EnqueueAction(eve, new ConvertItemAction(item2, item));
			}
			else
			{
				Effect.EnqueueAction(eve, new ExileItemAction(item2, eve.CasterItem, hitEffect));
			}
			hitEffect?.Play(eve, item2);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { combineTemplate, targetItemSelector };
	}
}
