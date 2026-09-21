using System.Collections.Generic;
using System.Linq;

public class TriggerFireControllerTableBurst : FireControllerAttackEffect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> tableItems = FireControllerExileEffect.GetTableItems(eve);
		Item casterItem = eve.CasterItem;
		if ((casterItem != null && casterItem.upgraded) || FireControllerExileEffect.IsSkinBranch(eve))
		{
			tableItems.AddRange(FireControllerExileEffect.GetExilePileItems(eve));
		}
		int damage = (from enchant in tableItems.Select(FireControllerExileEffect.GetBurstEnchant)
			where enchant != null
			select enchant).Sum((Enchant enchant) => enchant.value);
		FireControllerExileEffect.DealDamageToOpponent(eve, damage);
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		List<Item> tableItems = FireControllerExileEffect.GetTableItems(eve);
		Item casterItem = eve.CasterItem;
		bool flag = (casterItem != null && casterItem.upgraded) || FireControllerExileEffect.IsSkinBranch(eve);
		if (flag)
		{
			tableItems.AddRange(FireControllerExileEffect.GetExilePileItems(eve));
		}
		int baseDamage = (from enchant in tableItems.Select(FireControllerExileEffect.GetBurstEnchant)
			where enchant != null
			select enchant).Sum((Enchant enchant) => enchant.value);
		NodeDisplayInfo nodeDisplayInfo = FireControllerExileEffect.DamageDisplay(eve, baseDamage);
		nodeDisplayInfo.outputBool = new NodeDisplayInfo.Element<bool?>(flag);
		return nodeDisplayInfo;
	}
}
