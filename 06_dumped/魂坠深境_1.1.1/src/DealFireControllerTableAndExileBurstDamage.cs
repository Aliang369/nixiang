using System.Collections.Generic;
using System.Linq;

public class DealFireControllerTableAndExileBurstDamage : FireControllerAttackEffect
{
	public bool includeTable = true;

	public bool includeExilePile = true;

	public int damageMultiplierPercent = 100;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		if (includeTable)
		{
			list.AddRange(FireControllerExileEffect.GetTableItems(eve));
		}
		if (includeExilePile)
		{
			list.AddRange(FireControllerExileEffect.GetExilePileItems(eve));
		}
		int num = (from enchant in list.Select(FireControllerExileEffect.GetBurstEnchant)
			where enchant != null
			select enchant).Sum((Enchant enchant) => enchant.value);
		FireControllerExileEffect.DealDamageToOpponent(eve, num * damageMultiplierPercent / 100);
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> list = new List<Item>();
		if (includeTable)
		{
			list.AddRange(FireControllerExileEffect.GetTableItems(eve));
		}
		if (includeExilePile)
		{
			list.AddRange(FireControllerExileEffect.GetExilePileItems(eve));
		}
		int num = (from enchant in list.Select(FireControllerExileEffect.GetBurstEnchant)
			where enchant != null
			select enchant).Sum((Enchant enchant) => enchant.value);
		return FireControllerExileEffect.DamageDisplay(eve, num * damageMultiplierPercent / 100);
	}
}
