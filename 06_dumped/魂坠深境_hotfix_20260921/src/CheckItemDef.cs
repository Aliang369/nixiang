using System.Collections.Generic;
using UnityEngine;

public class CheckItemDef : CheckNode
{
	[SerializeField]
	private ItemTargetSelector itemTargetSelector;

	[SerializeField]
	private ItemDefGenerator def;

	public override bool? Check(GameEvent eve)
	{
		ItemDefinition itemDefinition = def.GenerateSingleItemDef(eve);
		if (itemDefinition == null)
		{
			return null;
		}
		List<Item> targetItems = itemTargetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		for (int i = 0; i < targetItems.Count; i++)
		{
			if (targetItems[i].GetDef() != itemDefinition)
			{
				return false;
			}
		}
		return true;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemTargetSelector, def };
	}
}
