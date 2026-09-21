using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConstraintSingleItemSelector : ItemTargetSelector
{
	public enum SelectType
	{
		InnerFirst,
		OuterFirst
	}

	[SerializeField]
	private GameEvent.EventBFTType bft;

	[SerializeField]
	private ItemConstraint constraint;

	[SerializeField]
	private GameEvent.OwnerItemsType itemsType;

	[SerializeField]
	private SelectType type;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { constraint };
	}

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		List<Item> eventItems = eve.GetEventItems(bft, itemsType);
		if (eventItems == null)
		{
			return list;
		}
		bool returnNull = false;
		eventItems = eventItems.FindAll(delegate(Item i)
		{
			if (constraint == null)
			{
				return true;
			}
			bool? flag = constraint.CheckItemAccept(i);
			if (!flag.HasValue)
			{
				returnNull = true;
			}
			return flag == true;
		});
		if (returnNull)
		{
			return null;
		}
		if (eventItems.Count == 0)
		{
			return list;
		}
		switch (type)
		{
		case SelectType.InnerFirst:
			list.Add(eventItems.First());
			break;
		case SelectType.OuterFirst:
			list.Add(eventItems.Last());
			break;
		}
		return list;
	}
}
