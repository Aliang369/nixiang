using System;
using System.Collections.Generic;
using System.Linq;

public class RandomConvertItemsToDefinitions : Effect
{
	public ItemTargetSelector targetItemSelector;

	public ItemDefGenerator itemDefGenerator;

	public ValueGenerator countGenerator;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (targetItemSelector == null || itemDefGenerator == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> targetItems = targetItemSelector.GetTargetItems(eve);
		List<ItemDefinition> list = itemDefGenerator.GenerateItemDefs(eve);
		if (targetItems == null || list == null)
		{
			return NodeCastResult.Failure;
		}
		targetItems = targetItems.Where((Item item) => item != null).Distinct().ToList();
		list = list.Where((ItemDefinition definition) => definition != null).Distinct().ToList();
		int? num = countGenerator?.GenerateValue(eve);
		if (countGenerator != null && !num.HasValue)
		{
			return NodeCastResult.Failure;
		}
		int val = num ?? list.Count;
		int num2 = Math.Min(Math.Max(0, val), Math.Min(targetItems.Count, list.Count));
		if (num2 == 0)
		{
			return NodeCastResult.Success;
		}
		GameRandomManager gameRandomManager = eve.gameState?.randomManager;
		if (gameRandomManager == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list2 = SelectRandomUnique(targetItems, num2, gameRandomManager);
		List<ItemDefinition> list3 = SelectRandomUnique(list, num2, gameRandomManager);
		for (int num3 = 0; num3 < num2; num3++)
		{
			Effect.EnqueueAction(eve, new ConvertItemToDefinitionAction(list2[num3], list3[num3]));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<ItemDefinition> list = itemDefGenerator?.GenerateItemDefs(eve);
		return new NodeDisplayInfo
		{
			refItemDefs = new NodeDisplayInfo.Element<List<ItemDefinition>>(list),
			times = new NodeDisplayInfo.Element<int?>(countGenerator?.GenerateValue(eve) ?? list?.Count)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetItemSelector, itemDefGenerator, countGenerator };
	}

	private static List<T> SelectRandomUnique<T>(IEnumerable<T> source, int count, GameRandomManager randomManager)
	{
		List<T> list = source.ToList();
		List<T> list2 = new List<T>(count);
		while (list2.Count < count)
		{
			int index = randomManager.Next(0, list.Count);
			list2.Add(list[index]);
			list.RemoveAt(index);
		}
		return list2;
	}
}
