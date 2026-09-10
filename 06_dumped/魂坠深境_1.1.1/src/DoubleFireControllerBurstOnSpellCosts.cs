using System.Collections.Generic;
using System.Linq;

public class DoubleFireControllerBurstOnSpellCosts : FireControllerExileEffect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = eve.spellInfo?.addedCosts;
		if (list == null)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in list.Where((Item item) => item != null).Distinct())
		{
			Enchant burstEnchant = FireControllerExileEffect.GetBurstEnchant(item);
			if (burstEnchant != null)
			{
				FireControllerExileEffect.AddEnchantToItem(eve, item, "a031fadc9104de2459ca595278678b17", burstEnchant.value);
			}
		}
		return NodeCastResult.Success;
	}
}
