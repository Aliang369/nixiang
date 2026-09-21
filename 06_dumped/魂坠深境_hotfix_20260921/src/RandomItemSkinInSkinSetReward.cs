using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class RandomItemSkinInSkinSetReward : GlobalRewardSetup
{
	public ItemSkinSetDef itemSkinSetDef;

	public int count;

	public bool ungot;

	public int batch;

	public List<ItemSkinDef> firstGrantExcludedSkins = new List<ItemSkinDef>();

	public ItemSkinDef secondGrantGuaranteedSkin;

	private string BatchDisplayName
	{
		get
		{
			object obj = itemSkinSetDef?.GetBatchDisplayName(batch);
			if (obj == null)
			{
				ItemSkinSetDef obj2 = itemSkinSetDef;
				if ((object)obj2 == null)
				{
					return null;
				}
				obj = obj2.skinSetName;
			}
			return (string)obj;
		}
	}

	private List<ItemSkinDef> skinInBatch
	{
		get
		{
			try
			{
				return itemSkinSetDef.GetSubSkins(Bootstrap.applicationState.assetCatalog, batch, ungot).ToList();
			}
			catch
			{
				return null;
			}
		}
	}

	public override string GlobalItemTitle => ScriptableObjectSingleton<TextDataPreset>.instance.term_random_card_skin_in_skin_set_batch_arg0.FormatWith(BatchDisplayName);

	public override ItemSkinSetDef GlobalItemSkinSetDef => itemSkinSetDef;

	public override int GlobalItemCount => count;

	public override QualityLevel GlobalItemQuality
	{
		get
		{
			if (!(itemSkinSetDef != null))
			{
				return QualityLevel.Common;
			}
			return itemSkinSetDef.quality;
		}
	}

	public override Sprite GlobalItemCellIcon => itemSkinSetDef.icon;

	public override string GetGlobalItemDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		generator = null;
		return (ungot ? ScriptableObjectSingleton<TextDataPreset>.instance.term_get_random_ungot_card_skin_in_skin_set_batch_arg0_with_count_arg0.FormatWith(BatchDisplayName, count) : ScriptableObjectSingleton<TextDataPreset>.instance.term_get_random_card_skin_in_skin_set_batch_arg0_with_count_arg0.FormatWith(BatchDisplayName, count)) + Environment.NewLine;
	}

	protected override GlobalRewardInfo GenerateRewardImpl(ApplicationStateInfo applicationState)
	{
		List<ItemSkinDef> subSkins = itemSkinSetDef.GetSubSkins(applicationState.assetCatalog, batch);
		int num = Mathf.Min(count, subSkins.Count);
		HashSet<ItemSkinDef> unlockedSkins = subSkins.Where(applicationState.itemSkinManager.IsItemSkinUnlocked).ToHashSet();
		List<ItemSkinDef> list = new List<ItemSkinDef>();
		if (num > 0 && unlockedSkins.Count == 1 && secondGrantGuaranteedSkin != null && subSkins.Contains(secondGrantGuaranteedSkin) && !unlockedSkins.Contains(secondGrantGuaranteedSkin))
		{
			list.Add(secondGrantGuaranteedSkin);
		}
		List<ItemSkinDef> list2 = (ungot ? subSkins.Where((ItemSkinDef skin) => !unlockedSkins.Contains(skin)).ToList() : subSkins.ToList());
		list2.RemoveAll(list.Contains);
		if (unlockedSkins.Count == 0 && firstGrantExcludedSkins != null)
		{
			HashSet<ItemSkinDef> excluded = firstGrantExcludedSkins.WhereNotNull().ToHashSet();
			List<ItemSkinDef> list3 = list2.Where((ItemSkinDef skin) => !excluded.Contains(skin)).ToList();
			if (list3.Count >= num - list.Count)
			{
				list2 = list3;
			}
		}
		int num2 = Mathf.Min(num - list.Count, list2.Count);
		list.AddRange(list2.SelectRandomCountUnique(num2));
		if (list.Count < num)
		{
			HashSet<ItemSkinDef> selectedSet = list.ToHashSet();
			List<ItemSkinDef> list4 = subSkins.Where((ItemSkinDef skin) => !selectedSet.Contains(skin)).ToList();
			int num3 = Mathf.Min(num - list.Count, list4.Count);
			list.AddRange(list4.SelectRandomCountUnique(num3));
		}
		return GenerateNestedReward(list);
	}

	private static GlobalRewardInfo GenerateNestedReward(IEnumerable<ItemSkinDef> skins)
	{
		GlobalRewardInfo globalRewardInfo = new GlobalRewardInfo();
		globalRewardInfo.nestedType = GlobalRewardInfo.RewardNestedType.RandomChest;
		globalRewardInfo.nestedRewards = new List<GlobalRewardInfo>();
		foreach (ItemSkinDef skin in skins)
		{
			GlobalRewardInfo globalRewardInfo2 = new GlobalRewardInfo();
			globalRewardInfo2.unlockItemSkin = skin;
			globalRewardInfo.nestedRewards.Add(globalRewardInfo2);
		}
		return globalRewardInfo;
	}

	public override string GetRewardBrief()
	{
		TextDataGenerator generator;
		return GetGlobalItemDescription(null, out generator);
	}
}
