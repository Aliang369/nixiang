using System.Linq;
using UnityEngine;

public class WithAnyEnchant : ItemConstraint
{
	[SerializeField]
	private bool negativeOnly;

	public override bool? CheckItemAccept(Item item)
	{
		bool? flag = base.CheckItemAccept(item);
		if (flag != true)
		{
			return flag;
		}
		return item.Enchants.Any((Enchant enchant) => !negativeOnly || enchant.def.type == EnchantDef.EnchantType.Negative);
	}
}
