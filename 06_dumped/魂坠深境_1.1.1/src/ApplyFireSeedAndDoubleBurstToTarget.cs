using System.Collections.Generic;

public class ApplyFireSeedAndDoubleBurstToTarget : FireControllerExileEffect
{
	public int fireSeedValue = 1;

	public int burstValue = 6;

	public int upgradedBurstValue = 12;

	public bool doubleExistingBurst;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item targetItem = eve.TargetItem;
		if (targetItem == null)
		{
			return NodeCastResult.Success;
		}
		EnchantDef enchantDef = FireControllerExileEffect.GetEnchantDef(eve, "97fb47d81f913754aa2ac35e19f09a57");
		EnchantDef enchantDef2 = FireControllerExileEffect.GetEnchantDef(eve, "a031fadc9104de2459ca595278678b17");
		if (enchantDef == null || enchantDef2 == null)
		{
			return NodeCastResult.Success;
		}
		if (eve.CasterItem != null && eve.CasterItem.owner == targetItem.owner)
		{
			Effect.EnqueueAction(eve, new SwapSlotItemAction(eve.CasterItem, targetItem, eve.CasterItem));
		}
		Effect.EnqueueAction(eve, new ChangeEnchantAction(targetItem, eve.CasterItem, enchantDef, FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 1)));
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(burstValue, 6), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBurstValue, 12));
		if (tierValue != 0)
		{
			Effect.EnqueueAction(eve, new ChangeEnchantAction(targetItem, eve.CasterItem, enchantDef2, tierValue));
		}
		Enchant burstEnchant = FireControllerExileEffect.GetBurstEnchant(targetItem);
		if (doubleExistingBurst && burstEnchant != null)
		{
			Effect.EnqueueAction(eve, new ChangeEnchantAction(targetItem, eve.CasterItem, enchantDef2, burstEnchant.value));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		EnchantDef enchantDef = FireControllerExileEffect.GetEnchantDef(eve, "97fb47d81f913754aa2ac35e19f09a57");
		EnchantDef enchantDef2 = FireControllerExileEffect.GetEnchantDef(eve, "a031fadc9104de2459ca595278678b17");
		List<Enchant> list = new List<Enchant>();
		if (enchantDef != null)
		{
			list.Add(new Enchant(enchantDef, FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 1)));
		}
		if (enchantDef2 != null)
		{
			list.Add(new Enchant(enchantDef2, FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(burstValue, 6), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBurstValue, 12))));
		}
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(burstValue, 6), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBurstValue, 12))),
			outputBool = new NodeDisplayInfo.Element<bool?>(eve.CasterItem?.upgraded ?? false),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(list)
		};
	}
}
