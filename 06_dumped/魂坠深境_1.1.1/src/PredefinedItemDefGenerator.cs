using System.Collections.Generic;

public class PredefinedItemDefGenerator : ItemDefGenerator
{
	public List<ItemDefinition> itemDefs = new List<ItemDefinition>();

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		return new List<ItemDefinition>(itemDefs);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(GenerateItemDefs(eve))
		};
	}
}
