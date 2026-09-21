using System.Collections.Generic;
using UnityEngine;

public class FilterPoolGenerator : ItemDefGenerator
{
	[SerializeField]
	private CheckNode checker;

	[SerializeField]
	private ItemConstraint filter;

	[SerializeField]
	private ItemDefGenerator pool;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		List<ItemDefinition> list = pool.GenerateItemDefs(eve);
		if (list == null)
		{
			return null;
		}
		List<Item> list2 = list.ConvertAll((ItemDefinition itd) => new Item(itd));
		if (filter != null)
		{
			bool returnNull = false;
			list2.RemoveAll(delegate(Item it)
			{
				bool? flag = filter.CheckItemAccept(it);
				if (!flag.HasValue)
				{
					returnNull = true;
				}
				return flag != true;
			});
			if (returnNull)
			{
				return null;
			}
		}
		if (checker != null)
		{
			bool returnNull2 = false;
			list2.RemoveAll(delegate(Item t)
			{
				eve.NodeTempItem = t;
				if (!checker.Check(eve).HasValue)
				{
					returnNull2 = true;
					return true;
				}
				return !checker.Check(eve).Value;
			});
		}
		return list2.ConvertAll((Item it) => it.GetDef());
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, filter, checker };
	}
}
