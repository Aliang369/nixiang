using UnityEngine;

public class DeckPresetCompDef
{
	[SaveField(null, null)]
	public ItemDefinition item;

	[SaveField(null, null)]
	public RuneDef rune;

	[SaveField(null, null)]
	public InfluenceDef influence;

	public DeckPresetCompType type
	{
		get
		{
			if (item != null)
			{
				return DeckPresetCompType.Item;
			}
			if (rune != null)
			{
				return DeckPresetCompType.Rune;
			}
			if (influence != null)
			{
				return DeckPresetCompType.Influence;
			}
			return DeckPresetCompType.None;
		}
	}

	public int DIYPoint => CompConfig?.diyPoint ?? 0;

	public PVPCompConfig CompConfig
	{
		get
		{
			if (item != null)
			{
				return item.pvpCompConfig;
			}
			if (influence != null)
			{
				return influence.pvpCompConfig;
			}
			if (rune != null)
			{
				return rune.pvpCompConfig;
			}
			return null;
		}
	}

	public int SpawnWeight => ScriptableObjectSingleton<PVPModeDateManager>.instance.pvpCompDrawWeightDic[CompConfig.spawnQuality];

	public Sprite DisplayIcon
	{
		get
		{
			if (item != null)
			{
				return item.Icon;
			}
			if (influence != null)
			{
				return influence.Icon;
			}
			if (rune != null)
			{
				return rune.Icon;
			}
			return null;
		}
	}

	public Sprite DisplayTypeIcon => ScriptableObjectSingleton<PVPModeDateManager>.instance.compTypeIconDic[type];

	public string DisplayName
	{
		get
		{
			if (item != null)
			{
				return item.itemName;
			}
			if (influence != null)
			{
				return influence.InfluenceName;
			}
			if (rune != null)
			{
				return rune.Name;
			}
			return null;
		}
	}

	public string TargetAssetID
	{
		get
		{
			if (item != null)
			{
				return item.FullID;
			}
			if (influence != null)
			{
				return influence.FullID;
			}
			if (rune != null)
			{
				return rune.FullID;
			}
			return "";
		}
	}

	public string UniqueID => "DECKPRESETCOMP_" + TargetAssetID;

	private DeckPresetCompDef()
	{
	}

	public DeckPresetCompDef(ItemDefinition itemDef)
	{
		item = itemDef;
	}

	public DeckPresetCompDef(RuneDef runeDef)
	{
		rune = runeDef;
	}

	public DeckPresetCompDef(InfluenceDef influenceDef)
	{
		influence = influenceDef;
	}

	public object GenerateTargetObject()
	{
		if (item != null)
		{
			return new Item(item);
		}
		if (rune != null)
		{
			return new Rune(rune, rune.attachGenerationInfo?.stack ?? 1);
		}
		if (influence != null)
		{
			return new Influence(influence);
		}
		return null;
	}
}
