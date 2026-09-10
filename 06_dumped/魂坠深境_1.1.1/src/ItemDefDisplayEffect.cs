using System.Collections.Generic;

public class ItemDefDisplayEffect : Effect
{
	public ItemDefinition itemDefinition;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(new List<ItemDefinition> { itemDefinition })
		};
	}
}
