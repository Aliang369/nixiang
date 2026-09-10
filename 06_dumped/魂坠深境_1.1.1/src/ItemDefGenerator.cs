using System.Collections.Generic;

public abstract class ItemDefGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(GenerateItemDefs(eve))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<ItemDefinition> GenerateItemDefs(GameEvent eve);

	public virtual ItemDefinition GenerateSingleItemDef(GameEvent eve)
	{
		List<ItemDefinition> list = GenerateItemDefs(eve);
		if (list != null && list.Count > 0)
		{
			return list[0];
		}
		return null;
	}
}
