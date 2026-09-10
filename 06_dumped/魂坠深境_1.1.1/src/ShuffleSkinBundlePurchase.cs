using System;
using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class ShuffleSkinBundlePurchase : PurchaseInfo
{
	public int period_offset;

	public ItemSkinSetBatch CurrentSkinSetBatch
	{
		get
		{
			List<ItemSkinSetBatch> skinPurchaseShuffle = ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseShuffle;
			if (skinPurchaseShuffle == null || skinPurchaseShuffle.Count == 0)
			{
				return null;
			}
			int index = GetCurrentPeriod() % skinPurchaseShuffle.Count;
			return skinPurchaseShuffle[index];
		}
	}

	public override string PurchaseTitleText => PurchaseInfo.textData.term_daily_rotating_card_pack;

	public override string PurchaseSubText => PurchaseInfo.textData.term_matter_count_arg0_arg1.FormatWith(CurrentSkinSetBatch?.DisplayName, count).NoBreak();

	public override Sprite GlobalItemCellIcon => CurrentSkinSetBatch?.skinSetDef?.icon;

	public override string GlobalItemTitle => PurchaseTitleText;

	public override string GetGlobalItemDescriptionTitle => PurchaseTitleText;

	public override ItemSkinSetDef GlobalItemSkinSetDef => CurrentSkinSetBatch?.skinSetDef;

	public override QualityLevel GlobalItemQuality => (CurrentSkinSetBatch?.skinSetDef?.quality).GetValueOrDefault();

	protected override int count => 5;

	private int GetCurrentPeriod()
	{
		return Math.Max(0, resetTimesType switch
		{
			ResetTimeType.Daily => GetDailyPeriod(DateTime.UtcNow), 
			ResetTimeType.Weekly => GetWeekPeriod(DateTime.UtcNow), 
			ResetTimeType.Monthly => GetMonthPeriod(DateTime.UtcNow), 
			ResetTimeType.Custom => ScriptableObjectSingleton<CalendarDataManager>.instance.GetCustomPeriod(DateTime.UtcNow, customResetDays), 
			_ => 0, 
		} - period_offset);
	}

	public override bool IsAllowPurchase(ApplicationStateInfo applicationState)
	{
		ItemSkinSetBatch currentSkinSetBatch = CurrentSkinSetBatch;
		if (currentSkinSetBatch == null)
		{
			return false;
		}
		if (currentSkinSetBatch.isAllUnlocked)
		{
			return false;
		}
		return base.IsAllowPurchase(applicationState);
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_get_random_ungot_card_skin_in_skin_set_batch_arg0_with_count_arg0.FormatWith(CurrentSkinSetBatch.DisplayName, 5);
	}

	private ShuffleSkinBundlePurchase()
	{
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		ItemSkinSetBatch currentSkinSetBatch = CurrentSkinSetBatch;
		if (currentSkinSetBatch == null)
		{
			return list;
		}
		List<ItemSkinDef> subSkins = currentSkinSetBatch.GetSubSkins();
		if (subSkins == null)
		{
			return list;
		}
		int num = Math.Min(count, subSkins.Count);
		foreach (ItemSkinDef item in subSkins.SelectRandomCountUnique(num))
		{
			if (item != null)
			{
				list.Add(new GlobalRewardInfo
				{
					unlockItemSkin = item
				});
			}
		}
		int num2 = count - num;
		if (num2 > 0)
		{
			Dictionary<QualityLevel, int> repeatItemSkinTokenCompensateDic = ScriptableObjectSingleton<FortressDataManager>.instance.repeatItemSkinTokenCompensateDic;
			QualityLevel qualityLevel = currentSkinSetBatch.skinSetDef?.quality ?? QualityLevel.Common;
			if (repeatItemSkinTokenCompensateDic == null || !repeatItemSkinTokenCompensateDic.TryGetValue(qualityLevel, out var value) || value <= 0)
			{
				TraceLog.Error($"卡牌形态品质{qualityLevel}未配置异界尘埃补偿");
				return list;
			}
			for (int i = 0; i < num2; i++)
			{
				list.Add(new GlobalRewardInfo
				{
					matter = new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.itemSkinTokenMatter, value),
					giveAppendInfo = new GlobalRewardGiveAppendInfo
					{
						repeated = true
					}
				});
			}
		}
		return list;
	}
}
