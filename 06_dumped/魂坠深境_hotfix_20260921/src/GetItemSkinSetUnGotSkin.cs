using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class GetItemSkinSetUnGotSkin : PurchaseInfo
{
	public ItemSkinSetDef itemSkinSet;

	public int batch;

	private string BatchDisplayName => itemSkinSet?.GetBatchDisplayName(batch);

	public override string PurchaseTitleText => BatchDisplayName;

	public override Sprite GlobalItemCellIcon => itemSkinSet.icon;

	public override string GetGlobalItemDescriptionTitle => PurchaseTitleText;

	public override ItemSkinSetDef GlobalItemSkinSetDef => itemSkinSet;

	public override QualityLevel GlobalItemQuality => itemSkinSet.quality;

	private List<ItemSkinDef> skinInBatch
	{
		get
		{
			try
			{
				return itemSkinSet.GetSubSkins(Bootstrap.applicationState.assetCatalog, batch, ungot: true).ToList();
			}
			catch
			{
				return null;
			}
		}
	}

	public override bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		if (itemSkinSet.IsAllUnlocked(applicationState))
		{
			return false;
		}
		return base.IsAllowPurchase(applicationState);
	}

	private ItemSkinDef SelectSkin(ApplicationStateInfo applicationState)
	{
		return (from s in itemSkinSet.GetSubSkins(applicationState.assetCatalog, batch, ungot: true)
			where !applicationState.itemSkinManager.IsItemSkinUnlocked(s)
			select s).ToList().SelectRandom();
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_random_ungot_card_skin_in_skin_set_batch_arg0_with_count_arg0.FormatWith(BatchDisplayName, 1);
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		ItemSkinDef unlockItemSkin = SelectSkin(applicationState);
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			unlockItemSkin = unlockItemSkin
		};
		return new List<GlobalRewardInfo> { item };
	}
}
