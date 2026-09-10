using System.Collections.Generic;

public class ConvertItemToDefinition : Effect
{
	public ItemTargetSelector targetItemSelector;

	public ItemDefinition itemDefinition;

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>((itemDefinition == null) ? null : new List<ItemDefinition> { itemDefinition })
		};
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (targetItemSelector == null || itemDefinition == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> targetItems = targetItemSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in targetItems)
		{
			if (item != null)
			{
				Effect.EnqueueAction(eve, new ConvertItemToDefinitionAction(item, itemDefinition));
			}
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetItemSelector };
	}
}
