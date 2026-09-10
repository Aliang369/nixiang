using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemSkinSetDef : CTScriptableObject
{
	public class SkinSetBatchConfig
	{
		public int batchIndex;

		[LocalizeText("职业名")]
		public string professionName;

		public int phaseIndex;

		[LocalizeText("显示名覆盖")]
		public string displayNameOverride;

		public bool hideInIndex;

		public List<ItemSkinDef> skinDefs;

		public Dictionary<int, int> unlockThresholdAndSetSkinCoinCost;

		public int BatchSize => skinDefs.Count;

		public string GetDisplayName(ItemSkinSetDef setDef, bool withDotSplit = false)
		{
			if (!string.IsNullOrWhiteSpace(displayNameOverride))
			{
				return displayNameOverride;
			}
			if (string.IsNullOrWhiteSpace(professionName))
			{
				if (!string.IsNullOrWhiteSpace(setDef?.skinSetName))
				{
					return ScriptableObjectSingleton<TextDataPreset>.instance.skin_batch.FormatWith($"{setDef.skinSetName}{batchIndex}");
				}
				return ScriptableObjectSingleton<TextDataPreset>.instance.skin_batch.FormatWith(batchIndex);
			}
			int phase = ((phaseIndex > 0) ? phaseIndex : batchIndex);
			if (!withDotSplit)
			{
				return professionName + FormatPhaseText(phase);
			}
			return professionName + "·" + FormatPhaseText(phase);
		}

		public int GetUnlockedCount(ApplicationStateInfo asi)
		{
			int num = 0;
			foreach (ItemSkinDef skinDef in skinDefs)
			{
				if (asi.itemSkinManager.IsItemSkinUnlocked(skinDef))
				{
					num++;
				}
			}
			return num;
		}

		public ItemSkinSetDef GetAttachSkinSet(ApplicationStateInfo asi)
		{
			return asi.assetCatalog.skinSetCatalog.ValidAssets.Find((ItemSkinSetDef s) => s.batchConfigs.Any((SkinSetBatchConfig bc) => bc == this));
		}

		public int? GetCurrentCollectLayer(ApplicationStateInfo asi)
		{
			if (GetAttachSkinSet(asi) == null)
			{
				return null;
			}
			int unlockedCount = GetUnlockedCount(asi);
			List<int> list = unlockThresholdAndSetSkinCoinCost.Keys.ToList();
			list.Sort();
			list.Reverse();
			foreach (int item in list)
			{
				if (unlockedCount >= item)
				{
					return item;
				}
			}
			return null;
		}

		public int? GetNextCollectLayer(ApplicationStateInfo asi)
		{
			if (GetAttachSkinSet(asi) == null)
			{
				return null;
			}
			int unlockedCount = GetUnlockedCount(asi);
			List<int> list = unlockThresholdAndSetSkinCoinCost.Keys.ToList();
			list.Sort();
			foreach (int item in list)
			{
				if (unlockedCount < item)
				{
					return item;
				}
			}
			return null;
		}

		public int GetCurrentSetSkinCoinCost(ApplicationStateInfo asi)
		{
			if (GetAttachSkinSet(asi) == null)
			{
				return 0;
			}
			int? currentCollectLayer = GetCurrentCollectLayer(asi);
			if (currentCollectLayer.HasValue)
			{
				return unlockThresholdAndSetSkinCoinCost[currentCollectLayer.Value];
			}
			return 0;
		}

		public int? GetNeedCollectCountForNextLayer(ApplicationStateInfo asi)
		{
			if (GetAttachSkinSet(asi) == null)
			{
				return null;
			}
			int unlockedCount = GetUnlockedCount(asi);
			int? nextCollectLayer = GetNextCollectLayer(asi);
			if (nextCollectLayer.HasValue)
			{
				return nextCollectLayer - unlockedCount;
			}
			return null;
		}
	}

	[Flags]
	public enum GatherType
	{
		Buy = 1
	}

	[Flags]
	public enum ItemSkinSetGetSourceType
	{
		Unknown = 0,
		DreamMirrorShop = 1,
		AscensionJourney = 2,
		Achievement = 4,
		StarChallenge = 8
	}

	[CTSOAssetName]
	[LocalizeText("卡牌皮肤系列名称")]
	public string skinSetName;

	[TextArea]
	[LocalizeText("卡牌皮肤系列描述")]
	public string description;

	[TextArea]
	[LocalizeText("卡牌皮肤系列获取方式")]
	public string gatherDescription;

	public Sprite icon;

	public Sprite background;

	public GatherType gatherType;

	public ItemSkinSetGetSourceType getSourceType;

	public ItemSkinBaseDef skinBase;

	public ItemSkinSetPanel itemSkinSetPanel;

	public QualityLevel quality;

	public Color skinSetMainColor;

	public List<SkinSetBatchConfig> batchConfigs;

	public override string CTAssetIDPrepend => "ITSKSET";

	public string SkinSetNameWithColor => skinSetName.ColorText(skinSetMainColor);

	private static string FormatPhaseText(int phase)
	{
		if (phase <= 0)
		{
			phase = 1;
		}
		(int, string)[] obj = new(int, string)[13]
		{
			(1000, "M"),
			(900, "CM"),
			(500, "D"),
			(400, "CD"),
			(100, "C"),
			(90, "XC"),
			(50, "L"),
			(40, "XL"),
			(10, "X"),
			(9, "IX"),
			(5, "V"),
			(4, "IV"),
			(1, "I")
		};
		string text = "";
		(int, string)[] array = obj;
		for (int i = 0; i < array.Length; i++)
		{
			(int, string) tuple = array[i];
			while (phase >= tuple.Item1)
			{
				text += tuple.Item2;
				phase -= tuple.Item1;
			}
		}
		return ScriptableObjectSingleton<TextDataPreset>.instance.skin_batch.FormatWith(text);
	}

	public List<ItemSkinDef> GetSubSkins(AssetCatalog catalog, int batch, bool ungot = false)
	{
		List<ItemSkinDef> list = catalog.itemSkinCatalog.ValidAssets.Where((ItemSkinDef s) => s.SetBatchIndex == batch && s.attachSkinSet == this).ToList();
		if (ungot)
		{
			List<ItemSkinDef> list2 = list.FindAll((ItemSkinDef s) => !Bootstrap.applicationState.itemSkinManager.IsItemSkinUnlocked(s));
			if (!list2.IsNullOrEmpty())
			{
				return list2;
			}
		}
		return list;
	}

	public List<ItemSkinDef> GetSubSkins(AssetCatalog catalog, bool ungot = false)
	{
		if (ungot)
		{
			List<ItemSkinDef> list = catalog.itemSkinCatalog.ValidAssets.FindAll((ItemSkinDef s) => s.attachSkinSet == this && !Bootstrap.applicationState.itemSkinManager.IsItemSkinUnlocked(s));
			if (!list.IsNullOrEmpty())
			{
				return list;
			}
		}
		return catalog.itemSkinCatalog.ValidAssets.FindAll((ItemSkinDef s) => s.attachSkinSet == this);
	}

	public List<(int, List<ItemSkinDef>)> GetBatchSkins(AssetCatalog catalog)
	{
		return (from d in GetSubSkins(catalog)
			group d by d.SetBatchIndex into g
			select (Key: g.Key, g.ToList())).ToList();
	}

	public List<SkinSetBatchConfig> GetIndexBatchConfigs()
	{
		return batchConfigs?.Where((SkinSetBatchConfig config) => config != null && !config.hideInIndex).ToList() ?? new List<SkinSetBatchConfig>();
	}

	public List<ItemSkinDef> GetIndexSubSkins(AssetCatalog catalog)
	{
		return GetSubSkins(catalog).FindAll(delegate(ItemSkinDef skin)
		{
			SkinSetBatchConfig setBatchConfig = skin.SetBatchConfig;
			return setBatchConfig == null || !setBatchConfig.hideInIndex;
		});
	}

	public SkinSetBatchConfig GetBatchConfig(int batch)
	{
		return batchConfigs?.FirstOrDefault((SkinSetBatchConfig c) => c.batchIndex == batch);
	}

	public string GetBatchDisplayName(int batch)
	{
		return skinSetName + "·" + GetBatchConfig(batch)?.GetDisplayName(this);
	}

	public bool IsAllUnlocked(ApplicationStateInfo asi)
	{
		return GetSubSkins(asi.assetCatalog).TrueForAll((ItemSkinDef s) => asi.itemSkinManager.IsItemSkinUnlocked(s));
	}

	public bool IsAllUnlocked(ApplicationStateInfo asi, int batch)
	{
		return GetSubSkins(asi.assetCatalog, batch).TrueForAll((ItemSkinDef s) => asi.itemSkinManager.IsItemSkinUnlocked(s));
	}

	public string GetGatherString()
	{
		string text = ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_gather_method;
		if (gatherType.HasFlag(GatherType.Buy))
		{
			text = text + "\n" + ScriptableObjectSingleton<TextDataPreset>.instance.term_purchase;
		}
		return text;
	}

	public string GetSourceText()
	{
		if (getSourceType == ItemSkinSetGetSourceType.Unknown)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_unknown;
		}
		List<string> list = new List<string>();
		if (getSourceType.HasFlag(ItemSkinSetGetSourceType.DreamMirrorShop))
		{
			list.Add(ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_dream_mirror_shop);
		}
		if (getSourceType.HasFlag(ItemSkinSetGetSourceType.AscensionJourney))
		{
			list.Add(ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_ascension_journey);
		}
		if (getSourceType.HasFlag(ItemSkinSetGetSourceType.Achievement))
		{
			list.Add(ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_achievement);
		}
		if (getSourceType.HasFlag(ItemSkinSetGetSourceType.StarChallenge))
		{
			list.Add(ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_star_challenge);
		}
		if (list.Count <= 0)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_cardskin_source_unknown;
		}
		return string.Join(" / ", list);
	}
}
