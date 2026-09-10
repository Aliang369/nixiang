using System.Collections.Generic;
using System.Linq;

public class ItemEnumerableValueGenerator : ValueGenerator
{
	public enum EnumType
	{
		Sum,
		Max,
		Min
	}

	public EnumType type;

	public ItemsTargetSelector itemsPool;

	public ValueGenerator valueGenerator;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> list = itemsPool?.GetTargetItems(eve);
		if (list.IsNullOrEmpty())
		{
			return 0;
		}
		List<int> list2 = new List<int>();
		Item nodeTempItem = eve.NodeTempItem;
		foreach (Item item in list)
		{
			eve.NodeTempItem = item;
			int? num = valueGenerator?.GenerateValue(eve);
			if (num.HasValue)
			{
				list2.Add(num.Value);
			}
		}
		eve.NodeTempItem = nodeTempItem;
		return type switch
		{
			EnumType.Sum => list2.Sum(), 
			EnumType.Max => list2.Max(), 
			EnumType.Min => list2.Min(), 
			_ => null, 
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemsPool, valueGenerator };
	}
}
