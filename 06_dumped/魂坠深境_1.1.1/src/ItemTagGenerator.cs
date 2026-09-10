using System.Collections.Generic;

public abstract class ItemTagGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<ItemTag> GenerateItemTags(GameEvent eve);

	public virtual ItemTag GenerateSingleItemTag(GameEvent eve)
	{
		List<ItemTag> list = GenerateItemTags(eve);
		if (list != null && list.Count > 0)
		{
			return list[0];
		}
		return null;
	}
}
