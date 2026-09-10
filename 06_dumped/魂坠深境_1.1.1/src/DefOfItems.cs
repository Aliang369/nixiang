using System.Collections.Generic;
using UnityEngine;

public class DefOfItems : ItemDefGenerator
{
	[SerializeField]
	private ItemTargetSelector itemsSelector;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		return itemsSelector.GetTargetItems(eve)?.ConvertAll((Item it) => it.GetDef());
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemsSelector };
	}
}
