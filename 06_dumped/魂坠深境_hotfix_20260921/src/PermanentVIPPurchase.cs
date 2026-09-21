using System.Collections.Generic;
using UnityEngine;

public class PermanentVIPPurchase : PurchaseInfo
{
	public Sprite icon;

	public override string PurchaseTitleText => PurchaseInfo.textData.term_PermanentVIPPurchase;

	public override Sprite GlobalItemCellIcon => icon;

	public override QualityLevel GlobalItemQuality => QualityLevel.Epic;

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			unlockPermanentVIP = true
		};
		return new List<GlobalRewardInfo> { item };
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return PurchaseInfo.textData.term_PermanentVIPPurchaseDescription;
	}
}
