using System.Collections.Generic;
using UnityEngine;

public class ItemsFilter : ItemTargetSelector
{
	[SerializeField]
	private CheckNode checkNode;

	[SerializeField]
	private ItemConstraint filter;

	[SerializeField]
	private ItemTargetSelector pool;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = pool.GetTargetItems(eve);
		if (list == null)
		{
			return null;
		}
		if (filter != null)
		{
			List<Item> list2 = list;
			list = new List<Item>();
			foreach (Item item2 in list2)
			{
				bool? flag = filter.CheckItemAccept(item2);
				if (!flag.HasValue)
				{
					return null;
				}
				if (flag == true)
				{
					list.Add(item2);
				}
			}
		}
		if (checkNode != null)
		{
			List<Item> list3 = new List<Item>();
			Item nodeTempItem = eve.NodeTempItem;
			foreach (Item item3 in list)
			{
				Item item = (eve.NodeTempItem = item3);
				bool? flag2 = checkNode.Check(eve);
				if (!flag2.HasValue)
				{
					return null;
				}
				if (flag2.Value)
				{
					list3.Add(item);
				}
			}
			eve.NodeTempItem = nodeTempItem;
			list = list3;
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, filter, checkNode };
	}
}
