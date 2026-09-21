using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NodeItemValueMostGenerator : ItemTargetSelector
{
	public enum SelectType
	{
		Max,
		Min,
		MaxAll,
		MinAll
	}

	[SerializeField]
	private ItemTargetSelector pool;

	[SerializeField]
	private SelectType type;

	[SerializeField]
	private ValueGenerator valueGenerator;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> targetItems = pool.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		List<Item> list = new List<Item>();
		List<(Item, int)> itemValuePairs = new List<(Item, int)>();
		Item nodeTempItem = eve.NodeTempItem;
		int item = 0;
		foreach (Item item3 in targetItems)
		{
			Item item2 = (eve.NodeTempItem = item3);
			if (valueGenerator != null)
			{
				int? num = valueGenerator.GenerateValue(eve);
				if (!num.HasValue)
				{
					return null;
				}
				item = num.Value;
			}
			itemValuePairs.Add((item2, item));
		}
		eve.NodeTempItem = nodeTempItem;
		itemValuePairs.Sort(((Item, int) x, (Item, int) y) => x.Item2.CompareTo(y.Item2));
		switch (type)
		{
		case SelectType.Max:
			list.Add(itemValuePairs.Last().Item1);
			break;
		case SelectType.Min:
			list.Add(itemValuePairs.First().Item1);
			break;
		case SelectType.MaxAll:
			list.AddRange(itemValuePairs.FindAll(((Item, int) t) => t.Item2 == itemValuePairs.Last().Item2).ConvertAll(((Item, int) t) => t.Item1));
			break;
		case SelectType.MinAll:
			list.AddRange(itemValuePairs.FindAll(((Item, int) t) => t.Item2 == itemValuePairs.First().Item2).ConvertAll(((Item, int) t) => t.Item1));
			break;
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { valueGenerator, pool };
	}
}
