using System.Collections.Generic;
using UnityEngine;

public class CheckItemsAcceptConsstraint : CheckNode
{
	public enum Op
	{
		And,
		Or
	}

	[SerializeField]
	private ItemConstraint checker;

	[SerializeField]
	private ItemTargetSelector itemSelector;

	[SerializeField]
	private Op op;

	public override bool? Check(GameEvent eve)
	{
		List<Item> list = itemSelector?.GetTargetItems(eve);
		if (list == null)
		{
			return null;
		}
		if (list.Count == 0)
		{
			return false;
		}
		switch (op)
		{
		case Op.And:
			foreach (Item item in list)
			{
				bool? flag2 = checker.CheckItemAccept(item);
				if (!flag2.HasValue)
				{
					return null;
				}
				if (flag2 == false)
				{
					return false;
				}
			}
			return true;
		case Op.Or:
			foreach (Item item2 in list)
			{
				bool? flag = checker.CheckItemAccept(item2);
				if (!flag.HasValue)
				{
					return null;
				}
				if (flag == true)
				{
					return true;
				}
			}
			return false;
		default:
			return false;
		}
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { itemSelector, checker };
	}
}
