using System.Collections.Generic;
using UnityEngine;

public class ItemsValueGenerator : ValueGenerator
{
	public enum OpType
	{
		Sum,
		Max,
		Min
	}

	[SerializeField]
	private ItemTargetSelector targetSelector;

	[SerializeField]
	private OpType type;

	[SerializeField]
	private ValueGenerator vg;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> targetItems = targetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		int num = 0;
		Item nodeTempItem = eve.NodeTempItem;
		foreach (Item item in targetItems)
		{
			eve.NodeTempItem = item;
			int? num2 = vg.GenerateValue(eve);
			if (!num2.HasValue)
			{
				return null;
			}
			switch (type)
			{
			case OpType.Sum:
				num += num2.Value;
				break;
			case OpType.Max:
				num = Mathf.Max(num2.Value, num);
				break;
			case OpType.Min:
				num = Mathf.Min(num2.Value, num);
				break;
			}
		}
		eve.NodeTempItem = nodeTempItem;
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, vg };
	}
}
