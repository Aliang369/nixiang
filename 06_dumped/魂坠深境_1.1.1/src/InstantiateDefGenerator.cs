using System.Collections.Generic;
using UnityEngine;

public class InstantiateDefGenerator : NewItemGenerator
{
	[SerializeField]
	public ItemDefGenerator defGenerator;

	[SerializeField]
	public ValueGenerator initCounterGenerator;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		List<ItemDefinition> list = defGenerator?.GenerateItemDefs(eve);
		if (list == null)
		{
			return null;
		}
		List<Item> list2 = list.FindAll((ItemDefinition d) => d != null).ConvertAll((ItemDefinition d) => new Item(d));
		if (initCounterGenerator != null)
		{
			int counter = initCounterGenerator.GenerateValue(eve) ?? (-1);
			list2.ForEach(delegate(Item i)
			{
				i.counter = counter;
			});
			list2.ForEach(delegate(Item i)
			{
				i.blackboard.SetValue(ItemBlackBoard.ValueType.InitCounter, counter);
			});
		}
		return list2;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { defGenerator, initCounterGenerator };
	}
}
