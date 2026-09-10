using System.Collections.Generic;
using UnityEngine;

public class EnchantCountGenerator : ValueGenerator
{
	[SerializeField]
	private EnchantDef enchant;

	[SerializeField]
	private GameEvent.EventItemType target;

	public override int? GenerateValue(GameEvent eve)
	{
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return null;
		}
		Enchant enchant = eventItem.Enchants?.Find((Enchant e) => e.def == this.enchant);
		if (enchant == null)
		{
			return 0;
		}
		return enchant.value;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<EnchantDef> value = ((enchant == null) ? null : new List<EnchantDef> { enchant });
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(GenerateValue(eve)),
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(value)
		};
	}
}
