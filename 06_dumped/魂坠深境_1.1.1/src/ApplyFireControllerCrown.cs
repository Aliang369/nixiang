using System.Collections.Generic;
using System.Linq;

public class ApplyFireControllerCrown : FireControllerExileEffect
{
	public int fireSeedValue = 1;

	public int upgradedFireSeedValue = 2;

	public int burstValue = 3;

	public int upgradedBurstValue = 6;

	public bool exileAfterCast = true;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 1), FireControllerExileEffect.UseConfiguredOrDefault(upgradedFireSeedValue, 2));
		int tierValue2 = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(burstValue, 3), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBurstValue, 6));
		foreach (Item item in from item in FireControllerExileEffect.GetTableItems(eve)
			where item != eve.CasterItem
			select item)
		{
			FireControllerExileEffect.AddEnchantToItem(eve, item, "97fb47d81f913754aa2ac35e19f09a57", tierValue);
			FireControllerExileEffect.AddEnchantToItem(eve, item, "a031fadc9104de2459ca595278678b17", tierValue2);
		}
		if (exileAfterCast && eve.CasterItem != null)
		{
			Effect.EnqueueAction(eve, new ExileItemAction(eve.CasterItem, eve.CasterItem));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int tierValue = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 1), FireControllerExileEffect.UseConfiguredOrDefault(upgradedFireSeedValue, 2));
		int tierValue2 = FireControllerExileEffect.GetTierValue(eve, FireControllerExileEffect.UseConfiguredOrDefault(burstValue, 3), FireControllerExileEffect.UseConfiguredOrDefault(upgradedBurstValue, 6));
		List<Enchant> list = new List<Enchant>();
		EnchantDef enchantDef = FireControllerExileEffect.GetEnchantDef(eve, "97fb47d81f913754aa2ac35e19f09a57");
		EnchantDef enchantDef2 = FireControllerExileEffect.GetEnchantDef(eve, "a031fadc9104de2459ca595278678b17");
		if (enchantDef != null)
		{
			list.Add(new Enchant(enchantDef, tierValue));
		}
		if (enchantDef2 != null)
		{
			list.Add(new Enchant(enchantDef2, tierValue2));
		}
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(tierValue),
			deltaValue = new NodeDisplayInfo.Element<int?>(tierValue2),
			outputBool = new NodeDisplayInfo.Element<bool?>(eve.CasterItem?.upgraded ?? false),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(list)
		};
	}
}
