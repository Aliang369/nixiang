using System.Collections.Generic;
using UnityEngine;

public class RealMoneyToMatterPurchase : PurchaseInfo
{
	public MatterInfo gain;

	public int bonusCount;

	public bool firstPurchaseDouble;

	public bool useMatterLootAfterPurchaseInsteadShow;

	public Sprite overrideSprite;

	public override string PurchaseTitleText => PurchaseInfo.FormatMatterPurchaseTitle(gain);

	public override Sprite GlobalItemCellIcon => overrideSprite ?? gain.matterDef.icon;

	public override string GlobalItemTitle => gain.matterDef.matterName;

	protected override int count => gain.count;

	public override QualityLevel GlobalItemQuality => QualityLevel.Epic;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return gain.matterDef.description;
	}

	public override int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(gain.matterDef);
	}

	private RealMoneyToMatterPurchase()
	{
	}

	public RealMoneyToMatterPurchase(int realMoneyCost, MatterInfo gain)
	{
		base.realMoneyCost = realMoneyCost;
		this.gain = gain;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		MatterInfo matterInfo = new MatterInfo(gain.matterDef, gain.count * purchaseTimes);
		if (firstPurchaseDouble && PurchasedCount(applicationState) == 0)
		{
			matterInfo.count += gain.count;
			purchaseTimes--;
		}
		if (bonusCount > 0)
		{
			matterInfo.count += bonusCount * purchaseTimes;
		}
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			matter = matterInfo
		};
		return new List<GlobalRewardInfo> { item };
	}
}
