using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class ShuffleSkinPurchase : PurchaseInfo
{
	public int period_offset;

	public Sprite icon;

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

	private string PoolDisplayName => string.Join(" / ", (from batch in ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseShuffle ?? new List<ItemSkinSetBatch>()
		where batch?.skinSetDef != null && !batch.DisplayName.IsNullOrEmpty()
		select batch.DisplayName).Distinct());

	public override string PurchaseTitleText => PurchaseInfo.textData.term_card_explore;

	public override string PurchaseSubText => null;

	public override Sprite GlobalItemCellIcon
	{
		get
		{
			object obj = icon;
			if (obj == null)
			{
				obj = PurchaseInfo.applicationState?.purchaseManager?.GetPurchaseGrouOfPurchase(purchaseId)?.purchases?.OfType<RealMoneyToMatterPurchase>().FirstOrDefault()?.gain?.matterDef?.icon;
				if (obj == null)
				{
					ItemSkinSetBatch currentSkinSetBatch = CurrentSkinSetBatch;
					if (currentSkinSetBatch == null)
					{
						return null;
					}
					ItemSkinSetDef skinSetDef = currentSkinSetBatch.skinSetDef;
					if ((object)skinSetDef == null)
					{
						return null;
					}
					obj = skinSetDef.icon;
				}
			}
			return (Sprite)obj;
		}
	}

	public override string GlobalItemTitle => PurchaseTitleText;

	public override string GetGlobalItemDescriptionTitle => PurchaseTitleText;

	public override ItemSkinSetDef GlobalItemSkinSetDef => null;

	public override QualityLevel GlobalItemQuality => CurrentSkinSetBatch?.skinSetDef.quality ?? QualityLevel.Common;

	private List<ItemSkinDef> GetUnownedSkinPool()
	{
		return (from skin in (ScriptableObjectSingleton<CalendarDataManager>.instance.skinPurchaseShuffle ?? new List<ItemSkinSetBatch>()).Where((ItemSkinSetBatch batch) => batch?.skinSetDef != null).SelectMany((ItemSkinSetBatch batch) => batch.GetSubSkins(ungot: false) ?? new List<ItemSkinDef>())
			where skin != null && !PurchaseInfo.applicationState.itemSkinManager.IsItemSkinUnlocked(skin)
			select skin).Distinct().ToList();
	}

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
		if (GetUnownedSkinPool().Count == 0)
		{
			return false;
		}
		return base.IsAllowPurchase(applicationState);
	}

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_daily_card_blind_box_description;
	}

	private ShuffleSkinPurchase()
	{
	}

	public override List<GlobalRewardInfo> GenerateReward(ApplicationStateInfo applicationState, int purchaseTimes)
	{
		List<GlobalRewardInfo> list = new List<GlobalRewardInfo>();
		List<ItemSkinDef> unownedSkinPool = GetUnownedSkinPool();
		int num = Math.Min(unownedSkinPool.Count, purchaseTimes);
		foreach (ItemSkinDef item in unownedSkinPool.SelectRandomCountUnique(num))
		{
			if (item != null)
			{
				list.Add(new GlobalRewardInfo
				{
					unlockItemSkin = item
				});
			}
		}
		return list;
	}
}
