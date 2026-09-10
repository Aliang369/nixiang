using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemsTargetSelector : ItemTargetSelector
{
	[SerializeField]
	private GameEvent.EventBFTType bft;

	[SerializeField]
	private GameEvent.OwnerItemsType itemsType;

	[SerializeField]
	private ItemConstraint filter;

	[SerializeField]
	private CheckNode checkNode;

	[SerializeField]
	private readonly bool ignoreSpellCost = true;

	[SerializeField]
	private bool unique;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = eve.GetEventItems(bft, itemsType);
		if (list == null)
		{
			return null;
		}
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay && itemsType != GameEvent.OwnerItemsType.Deck)
		{
			return null;
		}
		if (filter != null)
		{
			List<Item> list2 = list;
			list = new List<Item>();
			foreach (Item item in list2)
			{
				bool? flag = filter.CheckItemAccept(item);
				if (!flag.HasValue)
				{
					return null;
				}
				if (flag == true)
				{
					list.Add(item);
				}
			}
		}
		if (ignoreSpellCost)
		{
			list.RemoveAll((Item i) => i.IsAddAsSpellCost(eve.gameState));
		}
		if (checkNode != null)
		{
			List<Item> list3 = new List<Item>();
			Item nodeTempItem = eve.NodeTempItem;
			foreach (Item item2 in list)
			{
				eve.NodeTempItem = item2;
				bool? flag2 = checkNode.Check(eve);
				if (!flag2.HasValue)
				{
					return null;
				}
				if (flag2 == true)
				{
					list3.Add(item2);
				}
			}
			eve.NodeTempItem = nodeTempItem;
			list = list3;
		}
		HashSet<ItemDefinition> seen = new HashSet<ItemDefinition>();
		if (unique)
		{
			list = list.Where((Item x) => seen.Add(x.def)).ToList();
		}
		return list;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { filter, checkNode };
	}
}
