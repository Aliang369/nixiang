using System.Collections.Generic;
using UnityEngine;

public class SpellConsumCostSelector : ItemTargetSelector
{
	[SerializeField]
	private bool includeSpellCaster;

	public override List<Item> GetTargetItems(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		if (includeSpellCaster && eve.CasterItem != null)
		{
			list.Add(eve.CasterItem);
		}
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			if (!includeSpellCaster)
			{
				return null;
			}
			return list;
		}
		if (eve.spellInfo == null)
		{
			if (!includeSpellCaster)
			{
				return null;
			}
			return list;
		}
		foreach (Item addedCost in eve.spellInfo.addedCosts)
		{
			list.Add(addedCost);
		}
		return list;
	}
}
