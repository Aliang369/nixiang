using System.Collections.Generic;
using UnityEngine;

public class ItemKindGroupGenerator : ValueGenerator
{
	[SerializeField]
	public List<ItemConstraint> kindConstraints;

	[SerializeField]
	public ItemTargetSelector pool;

	public override int? GenerateValue(GameEvent eve)
	{
		List<Item> list = pool?.GetTargetItems(eve);
		if (list == null || kindConstraints == null)
		{
			return null;
		}
		int num = 0;
		foreach (ItemConstraint constraint in kindConstraints)
		{
			if (list.Find((Item it) => constraint.CheckItemAccept(it) == true) != null)
			{
				num++;
			}
		}
		return num;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (pool != null)
		{
			list.Add(pool);
		}
		if (kindConstraints != null)
		{
			list.AddRange(kindConstraints);
		}
		return list;
	}
}
