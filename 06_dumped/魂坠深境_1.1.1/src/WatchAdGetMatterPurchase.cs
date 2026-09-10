using System.Collections.Generic;
using UnityEngine;

public class WatchAdGetMatterPurchase : PurchaseInfo
{
	public enum Type
	{
		Daily,
		Weekly
	}

	public MatterInfo gain;

	public Type type;

	public override string PurchaseTitleText
	{
		get
		{
			if (type == Type.Daily)
			{
				return PurchaseInfo.textData.term_DailyAd;
			}
			return PurchaseInfo.textData.term_WeeklyAd;
		}
	}

	public override Sprite GlobalItemCellIcon => gain.matterDef.icon;

	protected override int count => gain.count;

	public override QualityLevel GlobalItemQuality => QualityLevel.Epic;

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			matter = gain
		};
		return new List<GlobalRewardInfo> { item };
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return gain.matterDef.description;
	}

	public override bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		if (base.IsAllowPurchase(applicationState))
		{
			return Bootstrap.instance.adsManager.CanClaimReward("free_money");
		}
		return false;
	}
}
