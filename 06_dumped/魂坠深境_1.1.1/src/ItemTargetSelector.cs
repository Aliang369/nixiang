using System.Collections.Generic;

public abstract class ItemTargetSelector : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> targetItems = GetTargetItems(eve);
		return new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>(targetItems),
			times = new NodeDisplayInfo.Element<int?>(targetItems?.Count)
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<Item> GetTargetItems(GameEvent eve);

	public Item GetSingleItem(GameEvent eve)
	{
		List<Item> targetItems = GetTargetItems(eve);
		if (targetItems == null || targetItems.Count == 0)
		{
			return null;
		}
		return targetItems[0];
	}
}
