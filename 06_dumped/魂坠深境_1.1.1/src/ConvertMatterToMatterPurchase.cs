using System.Collections.Generic;
using UnityEngine;

public class ConvertMatterToMatterPurchase : PurchaseInfo
{
	public MatterInfo gain;

	public override bool SupportBatchPurchase => true;

	public override string PurchaseTitleText => PurchaseInfo.FormatMatterPurchaseTitle(gain);

	public override Sprite GlobalItemCellIcon => gain.matterDef.icon;

	public override string GlobalItemTitle => gain.matterDef.matterName;

	public override QualityLevel GlobalItemQuality => gain.matterDef.qualityLevel;

	protected override int count => gain.count;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return gain.matterDef.description;
	}

	public override int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return applicationState.matterManager.GetMatterCount(gain.matterDef);
	}

	private ConvertMatterToMatterPurchase()
	{
	}

	public ConvertMatterToMatterPurchase(MatterInfo cost, MatterInfo gain)
	{
		mattercost = cost;
		this.gain = gain;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		MatterInfo matter = new MatterInfo(gain.matterDef, gain.count * purchaseTimes);
		GlobalRewardInfo item = new GlobalRewardInfo
		{
			matter = matter
		};
		return new List<GlobalRewardInfo> { item };
	}
}
