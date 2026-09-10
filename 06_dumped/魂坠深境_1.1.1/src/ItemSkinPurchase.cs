using System.Collections.Generic;
using UnityEngine;

public class ItemSkinPurchase : PurchaseInfo
{
	public ItemSkinDef itemSkin;

	public override string PurchaseTitleText => ScriptableObjectSingleton<TextDataPreset>.instance.term_item_of_itemskin_arg0.FormatWith(itemSkin.ItemSkinNameWithSet);

	public override Sprite GlobalItemCellIcon => itemSkin.globalItemIcon;

	public override string GlobalItemTitle => itemSkin.ItemSkinNameWithSet;

	public override ItemSkinDef GlobalItemRefItemSkin => itemSkin;

	public override QualityLevel GlobalItemQuality => itemSkin.quality;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_can_convert_item_arg0_into_itemskinset_arg1_with_skinset_description_arg2.FormatWith(itemSkin.attachItem.itemName, itemSkin.attachSkinSet.skinSetName, itemSkin.attachSkinSet.description);
	}

	public override bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		if (applicationState.itemSkinManager.IsItemSkinUnlocked(itemSkin))
		{
			return false;
		}
		return base.IsAllowPurchase(applicationState);
	}

	private ItemSkinPurchase()
	{
	}

	public ItemSkinPurchase(ItemSkinDef itemSkin, MatterInfo cost)
	{
		this.itemSkin = itemSkin;
		mattercost = cost;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			unlockItemSkin = itemSkin
		};
		return new List<GlobalRewardInfo> { item };
	}
}
