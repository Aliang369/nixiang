using System.Collections.Generic;
using UnityEngine;

public class CheckItemWithEnchant : CheckNode
{
	[SerializeField]
	private ItemTargetSelector itemTargetSelector;

	[SerializeField]
	private EnchantDef enchantDef;

	public override bool? Check(GameEvent eve)
	{
		List<Item> targetItems = itemTargetSelector.GetTargetItems(eve);
		if (targetItems == null)
		{
			return null;
		}
		for (int i = 0; i < targetItems.Count; i++)
		{
			IReadOnlyList<Enchant> enchants = targetItems[i].Enchants;
			if (enchants == null || enchants.Count == 0)
			{
				return false;
			}
			bool flag = false;
			for (int j = 0; j < enchants.Count; j++)
			{
				if (enchants[j].def == enchantDef)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return false;
			}
		}
		return true;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = base.GetNodeDisplayInfo(eve);
		nodeDisplayInfo.refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(new List<EnchantDef> { enchantDef });
		return nodeDisplayInfo;
	}
}
