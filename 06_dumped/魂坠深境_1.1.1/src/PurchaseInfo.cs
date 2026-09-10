using System;
using System.Collections.Generic;
using System.Linq;
using ChillyRoom.Services.ContainerOfSoul;
using ContainerOfSoul.Display.Fortress.Purchase;
using UnityEngine;

public abstract class PurchaseInfo : IGlobalItemDisplaySetup
{
	public string purchaseId;

	public MatterInfo mattercost;

	public int realMoneyCost;

	public int discount;

	public bool buyTimesLimit;

	public int repeatBuyTimes = 1;

	public int discountRepeatBuyTimes;

	public ResetTimeType resetTimesType;

	public int customResetDays;

	public int resetDay;

	public bool hideRangeSliderArea;

	public static ApplicationStateInfo applicationState => Bootstrap.applicationState;

	protected static TextDataPreset textData => ScriptableObjectSingleton<TextDataPreset>.instance;

	public abstract string PurchaseTitleText { get; }

	public virtual string PurchaseSubText => null;

	public virtual bool SupportBatchPurchase => false;

	public bool IsFreePurchase
	{
		get
		{
			if (mattercost == null)
			{
				return realMoneyCost == 0;
			}
			return false;
		}
	}

	public virtual Sprite GlobalItemCellIcon => ScriptableObjectSingleton<GameDataManager>.instance.itemPlaceHolderIcon;

	int IGlobalItemDisplaySetup.GlobalItemCount => count;

	protected virtual int count => 1;

	public virtual string GlobalItemCellName => PurchaseTitleText;

	public virtual string GlobalItemTitle => PurchaseTitleText;

	public virtual string GetGlobalItemDescriptionTitle => GlobalItemTitle;

	public bool HideNumberTextOnOnlyOne => true;

	public virtual QualityLevel GlobalItemQuality => QualityLevel.Common;

	public virtual ProfileSkill GlobalItemRefSkill => null;

	public virtual ItemSkinDef GlobalItemRefItemSkin => null;

	public virtual ItemSkinSetDef GlobalItemSkinSetDef => null;

	public virtual List<IGlobalItemDisplaySetup> RandomMatterDefs => null;

	public virtual List<IGlobalItemDisplaySetup> RandomStartGameBuffs => null;

	public virtual List<IGlobalItemDisplaySetup> RandomSkillDefs => null;

	public virtual PlayerCharacterDef GlobalItemRefPlayerCharacter => null;

	public virtual bool GlobalItemJumpToGachaDetail => false;

	public virtual bool isNew => false;

	public virtual bool JumpToGachaAnnouncement => false;

	public virtual bool JumpToStoreButton => false;

	protected static string FormatMatterPurchaseTitle(MatterInfo gain)
	{
		if (gain == null || gain.matterDef == null)
		{
			return string.Empty;
		}
		return TextDisplayHelper.FormatNameWithCount(gain.matterDef.matterName, gain.count);
	}

	public virtual string GetExtraHintText(ApplicationStateInfo applicationState)
	{
		return null;
	}

	public int GetDailyPeriod(DateTime time)
	{
		return ScriptableObjectSingleton<CalendarDataManager>.instance.GetDailyPeriod(time);
	}

	public int GetWeekPeriod(DateTime time)
	{
		return ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriod(time);
	}

	public int GetMonthPeriod(DateTime time)
	{
		return ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriod(time);
	}

	public int PurchasedCount(ApplicationStateInfo applicationState)
	{
		if (!applicationState.purchaseManager.purchasedOrderHistory.TryGetValue(purchaseId, out var value) || value.All((PurchaseRecord r) => r.orderStatus != OrderStatus.Paid))
		{
			return 0;
		}
		int num = 0;
		if (resetTimesType == ResetTimeType.Daily)
		{
			return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid && GetDailyPeriod(p.purchaseDateTime) == GetDailyPeriod(DateTime.UtcNow)).Sum((PurchaseRecord p) => p.quantity);
		}
		if (resetTimesType == ResetTimeType.Weekly)
		{
			return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid && GetWeekPeriod(p.purchaseDateTime) == GetWeekPeriod(DateTime.UtcNow)).Sum((PurchaseRecord p) => p.quantity);
		}
		if (resetTimesType == ResetTimeType.Monthly)
		{
			return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid && GetMonthPeriod(p.purchaseDateTime) == GetMonthPeriod(DateTime.UtcNow)).Sum((PurchaseRecord p) => p.quantity);
		}
		if (resetTimesType == ResetTimeType.Custom)
		{
			int currentPeriod = ScriptableObjectSingleton<CalendarDataManager>.instance.GetCustomPeriod(DateTime.UtcNow, customResetDays);
			return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid && ScriptableObjectSingleton<CalendarDataManager>.instance.GetCustomPeriod(p.purchaseDateTime, customResetDays) == currentPeriod).Sum((PurchaseRecord p) => p.quantity);
		}
		if (resetTimesType == ResetTimeType.Season)
		{
			return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid && applicationState.seasonManager.IsInSameSeason(p.purchaseDateTime, DateTime.UtcNow)).Sum((PurchaseRecord p) => p.quantity);
		}
		return value.Where((PurchaseRecord p) => p.orderStatus == OrderStatus.Paid).Sum((PurchaseRecord p) => p.quantity);
	}

	public bool HasPaidOrder(ApplicationStateInfo applicationState)
	{
		if (applicationState.purchaseManager.purchasedOrderHistory.TryGetValue(purchaseId, out var value))
		{
			return value.Any((PurchaseRecord r) => r.orderStatus == OrderStatus.Paid);
		}
		return false;
	}

	public static bool HasAnyPaidOrder(ApplicationStateInfo applicationState)
	{
		return applicationState.purchaseManager.purchasedOrderHistory.Values.Any((List<PurchaseRecord> records) => records.Any((PurchaseRecord r) => r.orderStatus == OrderStatus.Paid));
	}

	public int RemainPurchaseCount(ApplicationStateInfo applicationState)
	{
		return repeatBuyTimes - PurchasedCount(applicationState);
	}

	public bool HasDiscountPhase()
	{
		if (mattercost != null && realMoneyCost == 0 && discount > 0)
		{
			return discountRepeatBuyTimes > 0;
		}
		return false;
	}

	public int DiscountRemainPurchaseCount(ApplicationStateInfo applicationState)
	{
		if (!HasDiscountPhase())
		{
			return 0;
		}
		return Mathf.Max(0, discountRepeatBuyTimes - PurchasedCount(applicationState));
	}

	public bool IsDiscountStageActive(ApplicationStateInfo applicationState)
	{
		if (HasDiscountPhase())
		{
			return DiscountRemainPurchaseCount(applicationState) > 0;
		}
		return false;
	}

	public bool CurrentStageHasUnlimitedPurchaseCount(ApplicationStateInfo applicationState)
	{
		if (!buyTimesLimit)
		{
			return !IsDiscountStageActive(applicationState);
		}
		return false;
	}

	public int CurrentStageRemainPurchaseCount(ApplicationStateInfo applicationState)
	{
		if (IsDiscountStageActive(applicationState))
		{
			return DiscountRemainPurchaseCount(applicationState);
		}
		if (!buyTimesLimit)
		{
			return -1;
		}
		return Mathf.Max(0, RemainPurchaseCount(applicationState));
	}

	public MatterInfo CurrentMatterCost(ApplicationStateInfo applicationState)
	{
		if (mattercost == null)
		{
			return null;
		}
		if (!HasDiscountPhase() || IsDiscountStageActive(applicationState))
		{
			return mattercost;
		}
		if (!TryBuildOriginalMatterCost(out var originalMatterCost))
		{
			return mattercost;
		}
		return originalMatterCost;
	}

	public int CurrentDiscountPercent(ApplicationStateInfo applicationState)
	{
		if (mattercost == null || discount <= 0)
		{
			return 0;
		}
		if (HasDiscountPhase() && !IsDiscountStageActive(applicationState))
		{
			return 0;
		}
		return discount;
	}

	public bool PurchaseCountEnough(ApplicationStateInfo applicationState)
	{
		if (!CurrentStageHasUnlimitedPurchaseCount(applicationState))
		{
			return CurrentStageRemainPurchaseCount(applicationState) > 0;
		}
		return true;
	}

	public DateTime? GetResetEndTime()
	{
		return resetTimesType switch
		{
			ResetTimeType.Daily => ScriptableObjectSingleton<CalendarDataManager>.instance.GetDayPeriodEndTime().UtcDateTime, 
			ResetTimeType.Weekly => ScriptableObjectSingleton<CalendarDataManager>.instance.GetWeekPeriodEndTime().UtcDateTime, 
			ResetTimeType.Monthly => ScriptableObjectSingleton<CalendarDataManager>.instance.GetMonthPeriodEndTime().UtcDateTime, 
			ResetTimeType.Custom => ScriptableObjectSingleton<CalendarDataManager>.instance.GetCustomPeriodEndTime(customResetDays).UtcDateTime, 
			ResetTimeType.Season => applicationState.seasonManager.GetCurrentSeasonEndTimeUtc(), 
			_ => null, 
		};
	}

	public abstract List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes);

	public bool IsEnoughCost(ApplicationStateInfo applicationState, int count = 1)
	{
		MatterInfo matterInfo = CurrentMatterCost(applicationState);
		if (matterInfo != null && !(matterInfo * count).IsPayable(applicationState))
		{
			return false;
		}
		return true;
	}

	public virtual bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		if (resetTimesType == ResetTimeType.MonthlyCardExpired)
		{
			return !applicationState.playerInfoManager.IsMonthlyCardActive;
		}
		return PurchaseCountEnough(applicationState);
	}

	public static string FormatLimitedTimeBundleCountdown(TimeSpan remainTime)
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_limited_time_bundle_countdown_prefix + TextDisplayHelper.GetAdaptiveCountdownText(remainTime);
	}

	public string GetLimitedTimeBundleCountdownText(ApplicationStateInfo applicationState)
	{
		if (applicationState?.purchaseManager?.limitedTimeBundleInfos == null)
		{
			return null;
		}
		if (!(this is PurchaseCell.ILimitedDisplayTime))
		{
			return null;
		}
		LimitedTimeBundleInfo limitedTimeBundleInfo = applicationState.purchaseManager.limitedTimeBundleInfos.FirstOrDefault((LimitedTimeBundleInfo info) => info != null && string.Equals(info.PurchaseId, purchaseId, StringComparison.Ordinal) && info.EndTimeUtc.HasValue);
		if (limitedTimeBundleInfo == null || !limitedTimeBundleInfo.EndTimeUtc.HasValue)
		{
			return null;
		}
		TimeSpan timeSpan = limitedTimeBundleInfo.EndTimeUtc.Value - DateTime.UtcNow;
		if (timeSpan < TimeSpan.Zero)
		{
			timeSpan = TimeSpan.Zero;
		}
		return FormatLimitedTimeBundleCountdown(timeSpan);
	}

	public bool TryBuildOriginalMatterCost(out MatterInfo originalMatterCost)
	{
		originalMatterCost = null;
		if (mattercost == null || mattercost.matterDef == null || discount <= 0)
		{
			return false;
		}
		int num = 100 - discount;
		if (num <= 0)
		{
			return false;
		}
		long num2 = (long)mattercost.count * 100L;
		if (num2 % num != 0L)
		{
			return false;
		}
		long num3 = num2 / num;
		if (num3 <= 0 || num3 > int.MaxValue)
		{
			return false;
		}
		originalMatterCost = new MatterInfo(mattercost.matterDef, (int)num3);
		return true;
	}

	public virtual string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return null;
	}

	public virtual int? GetGlobalItemStorageCount(ApplicationStateInfo applicationState)
	{
		return null;
	}
}
