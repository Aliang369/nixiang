using System.Collections.Generic;

public class SetItemRemoveAfterCast : Effect
{
	public ItemTargetSelector targetSelector;

	public bool value = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = targetSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			item?.SetRemoveAfterCast(value);
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector };
	}
}
