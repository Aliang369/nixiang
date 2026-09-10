using System.Collections.Generic;

public abstract class NewItemGenerator : IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public virtual NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> list = GenerateNewItems(eve);
		return new NodeDisplayInfo
		{
			refItems = new NodeDisplayInfo.Element<List<Item>>(list),
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(list?.ConvertAll((Item n) => n.GetDef()))
		};
	}

	public virtual List<IEffectNode> GetChildren()
	{
		return null;
	}

	public abstract List<Item> GenerateNewItems(GameEvent eve);
}
