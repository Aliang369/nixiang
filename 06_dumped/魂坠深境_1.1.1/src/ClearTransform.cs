using System.Collections.Generic;

public class ClearTransform : Effect
{
	public ItemTargetSelector itemSelector;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = itemSelector?.GetTargetItems(eve);
		if (list == null || list.Count == 0)
		{
			return NodeCastResult.Failure;
		}
		bool flag = false;
		foreach (Item item in list)
		{
			if (item != null)
			{
				Effect.EnqueueAction(eve, new ClearTransformAction(item));
				flag = true;
			}
		}
		if (!flag)
		{
			return NodeCastResult.Failure;
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return IEffectNode.ToList(itemSelector);
	}
}
