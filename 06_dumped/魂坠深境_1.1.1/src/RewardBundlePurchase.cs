using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using ContainerOfSoul.Display.Fortress.Purchase;
using UnityEngine;

public class RewardBundlePurchase : PurchaseInfo, PurchaseCell.ILimitedDisplayTime
{
	public GlobalRewardBundle rewardBundle;

	public bool limitedDisplayTime;

	public override string PurchaseTitleText => rewardBundle.title;

	public override string GlobalItemTitle => rewardBundle.title;

	bool PurchaseCell.ILimitedDisplayTime.isValid
	{
		get
		{
			if (limitedDisplayTime)
			{
				if (!PurchaseInfo.HasAnyPaidOrder(PurchaseInfo.applicationState))
				{
					ApplicationStateInfo applicationStateInfo = PurchaseInfo.applicationState;
					if (applicationStateInfo == null)
					{
						return false;
					}
					return applicationStateInfo.purchaseManager?.limitedTimeBundleInfos?.Any((LimitedTimeBundleInfo info) => info != null && string.Equals(info.PurchaseId, purchaseId, StringComparison.Ordinal) && info.EndTimeUtc.HasValue && info.EndTimeUtc.Value > DateTime.UtcNow) == true;
				}
				return false;
			}
			return true;
		}
	}

	public override Sprite GlobalItemCellIcon => rewardBundle.icon;

	public override QualityLevel GlobalItemQuality
	{
		get
		{
			if (!(rewardBundle != null))
			{
				return QualityLevel.Epic;
			}
			return rewardBundle.displayQuality;
		}
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return rewardBundle.description;
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		return GlobalRewardInfo.UnfoldNestedReward(rewardBundle.rewards.Select((GlobalRewardSetup r) => r.GenerateReward(applicationState, GlobalRewardSourceType.PurchaseBundle, purchaseId)).ToList());
	}

	private RewardBundlePurchase()
	{
	}

	public RewardBundlePurchase(GlobalRewardBundle rewardBundle)
	{
		this.rewardBundle = rewardBundle;
	}

	private bool IsMonthlyCardBundle()
	{
		return rewardBundle.rewards.Any((GlobalRewardSetup r) => r is UnlockMonthlyCardSetup);
	}

	public override string GetExtraHintText(ApplicationStateInfo applicationState)
	{
		if (IsMonthlyCardBundle() && applicationState.playerInfoManager.IsMonthlyCardActive)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_month_card_remain_arg0_days.FormatWith(applicationState.playerInfoManager.RemainingMonthlyCardDays);
		}
		return null;
	}
}
