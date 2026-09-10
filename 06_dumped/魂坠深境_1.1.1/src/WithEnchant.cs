using System.Collections.Generic;
using UnityEngine;

public class WithEnchant : ItemConstraint
{
	[SerializeField]
	private EnchantDef ecd;

	public override bool? CheckItemAccept(Item item)
	{
		if (ecd == null || item == null)
		{
			return null;
		}
		return item.Enchants.Find((Enchant e) => e.def == ecd) != null;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<EnchantDef> value = ((ecd == null) ? null : new List<EnchantDef> { ecd });
		return new NodeDisplayInfo
		{
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>(value)
		};
	}
}
