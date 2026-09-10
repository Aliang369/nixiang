using System.Collections.Generic;

public class ApplyFireControllerHeatToTable : FireControllerExileEffect
{
	public int fireSeedValue = 3;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item targetItem = eve.TargetItem;
		if (targetItem == null)
		{
			return NodeCastResult.Success;
		}
		int delta = (FireControllerExileEffect.IsSkinBranch(eve) ? FireControllerExileEffect.GetTableItems(eve).Count : FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 3));
		FireControllerExileEffect.AddEnchantToItem(eve, targetItem, "97fb47d81f913754aa2ac35e19f09a57", delta);
		Item casterItem = eve.CasterItem;
		if ((casterItem == null || !casterItem.upgraded) && !FireControllerExileEffect.IsSkinBranch(eve) && eve.CasterItem != null)
		{
			Effect.EnqueueAction(eve, new ExileItemAction(eve.CasterItem, eve.CasterItem));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		Item casterItem = eve.CasterItem;
		bool value = (casterItem != null && casterItem.upgraded) || FireControllerExileEffect.IsSkinBranch(eve);
		int value2 = (FireControllerExileEffect.IsSkinBranch(eve) ? FireControllerExileEffect.GetTableItems(eve).Count : FireControllerExileEffect.UseConfiguredOrDefault(fireSeedValue, 3));
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(value2),
			outputBool = new NodeDisplayInfo.Element<bool?>(value),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>(FireControllerExileEffect.EnchantDisplay(eve, "97fb47d81f913754aa2ac35e19f09a57", value2))
		};
	}
}
