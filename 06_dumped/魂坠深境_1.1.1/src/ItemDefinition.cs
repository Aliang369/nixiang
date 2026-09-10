using System.Collections.Generic;
using System.Linq;
using OdinEx;
using UnityEngine;

public class ItemDefinition : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public class ItemSkinConfig
	{
		public ItemSkinDef itemSkinDef;

		[TextAreaProp(1, 5, false, 0)]
		[LocalizeText("卡牌主动效果描述")]
		public string spellFormatDescription;

		[TextAreaProp(1, 5, false, 0)]
		[LocalizeText("卡牌主动效果描述")]
		public string upgradedSpellFormatDescription;

		[TextAreaProp(1, 5, false, 0)]
		[LocalizeText("卡牌被动效果描述")]
		public string passiveFormatDescription;

		public bool removeSpell;

		public bool overrideTags;

		public List<ItemTag> tags;

		public bool overrideWithCounter;

		public bool withCounter;

		public bool overrideInitMaxCounter;

		public int initMaxCounter;

		public bool overrideInitCounter;

		public int initCounter;

		public ItemSpell overrideSpell;

		public IOutGameEffectNode upgradeEffect;

		public IOutGameEffectNode downgradeEffect;

		public string GetSpellFormatDescription(bool upgraded)
		{
			if (!upgraded || string.IsNullOrEmpty(upgradedSpellFormatDescription))
			{
				return spellFormatDescription;
			}
			return upgradedSpellFormatDescription;
		}
	}

	public bool hideInIndex;

	public bool includeInExtraIndex;

	[SerializeField]
	protected Sprite _icon;

	[SerializeField]
	public List<ItemTag> tags;

	public bool withCounter;

	public int initMaxCounter;

	public int initCounter;

	public QualityLevel qualityLevel;

	public bool upgradeable;

	public bool infiniteUpgrade;

	public bool isTransformSpell;

	public bool holdRepeatCastEnabled;

	public HoldRepeatCastConfig holdRepeatCastConfig;

	public ItemBlackBoard defaultBlackBoard;

	[LocalizeText("卡牌名字（翻译时应当尽可能简短，最好不超过2个单词，可以适当传神意译）")]
	[CTSOAssetName]
	public string itemName;

	[LocalizeText("卡牌名字（翻译时应当尽可能简短，最好不超过2个单词，可以适当传神意译）")]
	public string upgradedItemName;

	[LocalizeText("卡牌背景故事")]
	[TextAreaProp(1, 5, false, 0)]
	public string itemStory;

	[TextAreaProp(1, 5, false, 0)]
	[LocalizeText("卡牌主动效果描述")]
	public string spellFormtDescription;

	[TextAreaProp(1, 5, false, 0)]
	[LocalizeText("卡牌被动效果描述")]
	public string passiveFormtDescription;

	[TextAreaProp(1, 5, false, 0)]
	[LocalizeText("卡牌被动效果描述")]
	public string upgradedPassiveFormtDescription;

	[TextAreaProp(1, 5, false, 0)]
	[LocalizeText("卡牌变形效果描述")]
	public string configuredTransformSpellDescription;

	public Sprite ringSprite;

	public bool burnCost;

	public List<EnchantDef> inherentEnchants;

	public List<ItemSpell> itemSpells;

	public ItemSpell configuredTransformSpellTemplate;

	public List<Ability> abilities;

	public IOutGameEffectNode upgradeEffect;

	public IOutGameEffectNode downgradeEffect;

	public List<ItemSkinConfig> itemSkinConfigs;

	public int treasureValue;

	public ItemGenerateionInfo generationInfo;

	public PVPCompConfig pvpCompConfig;

	public override string CTAssetIDPrepend => "IT";

	public Sprite Icon
	{
		get
		{
			if (_icon != null)
			{
				return _icon;
			}
			return ScriptableObjectSingleton<GameDataManager>.instance?.itemPlaceHolderIcon;
		}
	}

	public bool Health => tags.Contains(ScriptableObjectSingleton<GameDataManager>.instance.healthTag);

	public bool Companion => tags.Contains(ScriptableObjectSingleton<GameDataManager>.instance.companionTag);

	public bool DefenceCompanion => tags.Contains(ScriptableObjectSingleton<GameDataManager>.instance.defenceCompanionTag);

	public bool Defence => tags.Contains(ScriptableObjectSingleton<GameDataManager>.instance.defendTag);

	public ItemSpell defaultSpell
	{
		get
		{
			if (itemSpells != null && itemSpells.Count != 0)
			{
				return itemSpells[0];
			}
			return null;
		}
	}

	public bool WithSpell
	{
		get
		{
			if (itemSpells != null)
			{
				return itemSpells.Count > 0;
			}
			return false;
		}
	}

	public bool IsPVPComp => pvpCompConfig != null;

	public int cost
	{
		get
		{
			if (itemSpells.IsNullOrEmpty())
			{
				return -1;
			}
			return itemSpells[0].requirement.costCount;
		}
	}

	public Sprite IconAsset => _icon;

	public ItemSkinConfig GetItemSkinConfig(ItemSkinDef skin)
	{
		return itemSkinConfigs?.FirstOrDefault((ItemSkinConfig c) => c.itemSkinDef != null && c.itemSkinDef == skin);
	}

	public List<ItemTag> GetTags(ItemSkinDef skin = null)
	{
		ItemSkinConfig itemSkinConfig = GetItemSkinConfig(skin);
		if (itemSkinConfig == null || !itemSkinConfig.overrideTags || itemSkinConfig.tags == null)
		{
			return tags;
		}
		return itemSkinConfig.tags;
	}

	public ItemTag GetMajorTag(ItemSkinDef skin = null)
	{
		return GetTags(skin)?.Find((ItemTag t) => t?.majorTag ?? false);
	}

	public bool GetWithCounter(ItemSkinDef skin = null)
	{
		ItemSkinConfig itemSkinConfig = GetItemSkinConfig(skin);
		if (itemSkinConfig == null || !itemSkinConfig.overrideWithCounter)
		{
			return withCounter;
		}
		return itemSkinConfig.withCounter;
	}

	public int GetInitMaxCounter(ItemSkinDef skin = null)
	{
		if (!GetWithCounter(skin))
		{
			return 0;
		}
		ItemSkinConfig itemSkinConfig = GetItemSkinConfig(skin);
		return Mathf.Max(0, (itemSkinConfig != null && itemSkinConfig.overrideInitMaxCounter) ? itemSkinConfig.initMaxCounter : initMaxCounter);
	}

	public int GetInitCounter(ItemSkinDef skin = null)
	{
		if (!GetWithCounter(skin))
		{
			return 0;
		}
		ItemSkinConfig itemSkinConfig = GetItemSkinConfig(skin);
		int num = ((itemSkinConfig != null && itemSkinConfig.overrideInitCounter) ? itemSkinConfig.initCounter : initCounter);
		int num2 = GetInitMaxCounter(skin);
		if (num2 <= 0)
		{
			return Mathf.Max(0, num);
		}
		return Mathf.Clamp(num, 0, num2);
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (itemSpells != null)
		{
			foreach (ItemSpell itemSpell in itemSpells)
			{
				list.AddRange(itemSpell.GetChildren());
			}
		}
		if (configuredTransformSpellTemplate != null)
		{
			if (configuredTransformSpellTemplate.requirement?.targetSelector != null)
			{
				list.Add(configuredTransformSpellTemplate.requirement.targetSelector);
			}
			if (configuredTransformSpellTemplate.requirement?.castCheckNode != null)
			{
				list.Add(configuredTransformSpellTemplate.requirement.castCheckNode);
			}
			if (configuredTransformSpellTemplate.requirement?.targetCheckNode != null)
			{
				list.Add(configuredTransformSpellTemplate.requirement.targetCheckNode);
			}
			if (configuredTransformSpellTemplate.requirement?.targetRequirement != null)
			{
				list.Add(configuredTransformSpellTemplate.requirement.targetRequirement);
			}
			if (configuredTransformSpellTemplate.spellNode != null)
			{
				list.Add(configuredTransformSpellTemplate.spellNode);
			}
		}
		if (abilities != null && abilities.Count > 0)
		{
			foreach (Ability ability in abilities)
			{
				if (ability != null)
				{
					list.Add(ability);
				}
			}
		}
		if (upgradeEffect != null)
		{
			list.Add(upgradeEffect);
		}
		if (downgradeEffect != null)
		{
			list.Add(downgradeEffect);
		}
		foreach (ItemSkinConfig item in itemSkinConfigs.OrEmptyIfNull())
		{
			if (item.overrideSpell != null)
			{
				list.AddRange(item.overrideSpell.GetChildren());
			}
			if (item.upgradeEffect != null)
			{
				list.Add(item.upgradeEffect);
			}
			if (item.downgradeEffect != null)
			{
				list.Add(item.downgradeEffect);
			}
		}
		return list;
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public bool HasTag(ItemTag tag, ItemSkinDef skin = null)
	{
		return GetTags(skin)?.Contains(tag) ?? false;
	}

	public string GetTagsText()
	{
		string text = "";
		foreach (ItemTag tag in tags)
		{
			text += tag?.GetNameText();
		}
		return text;
	}

	public bool HasIcon()
	{
		return _icon != null;
	}

	public bool IsUnlocked(ApplicationStateInfo applicationState)
	{
		return applicationState.globalUnlockManager.IsUnlocked(this);
	}

	public bool IsDiscovered(ApplicationStateInfo applicationState)
	{
		return applicationState.discoverManager.IsDiscovered(this);
	}

	public static int ItemDefIndexComparer(ItemDefinition a, ItemDefinition b, ApplicationStateInfo applicationState = null)
	{
		if (applicationState != null)
		{
			int num = b.IsUnlocked(applicationState).CompareTo(a.IsUnlocked(applicationState));
			if (num != 0)
			{
				return num;
			}
			int num2 = b.IsDiscovered(applicationState).CompareTo(a.IsDiscovered(applicationState));
			if (num2 != 0)
			{
				return num2;
			}
		}
		int num3 = a.qualityLevel.CompareTo(b.qualityLevel);
		if (num3 != 0)
		{
			return num3;
		}
		return a.SOindex.CompareTo(b.SOindex);
	}
}
