using Proto.Enums;
using Proto.Types;
using UnityEngine;
using UnityEngine.UIElements;

public static class zbResourceHelper
{
	private const string DefaultIconPath = "ui/image/common/transparent.png";

	private static readonly string[] EResourceQualityFrame;

	private static readonly string[] EResourceQualityBg;

	private static readonly string[] EHeroQualityBg;

	private static readonly string[] EShopQualityBg;

	private static readonly string[] EShopVipQualityBg;

	private static readonly Color[] EColor;

	public static void RefreshResourceItem(EResourceType awardType, long awardId, long awardCount, VisualElement spriteFrame, VisualElement spriteIcon, Label lbName, Label lbCount)
	{
		if (awardId > 0)
		{
			string resourceFrameTex = GetResourceFrameTex(GetResourceQuality(awardType, awardId));
			stResorceIconInfo resorceIconPath = GetResorceIconPath(awardType, awardId);
			SetSpriteBackground(spriteFrame, resourceFrameTex);
			SetSpriteBackground(spriteIcon, resorceIconPath.strIconPath);
			((TextElement)lbName).text = resorceIconPath.strName;
			((TextElement)lbCount).text = awardCount.ToString();
		}
		else
		{
			string path = "ui/image/frame/frame_1.png";
			SetSpriteBackground(spriteFrame, path);
			SetSpriteBackground(spriteIcon, "ui/image/common/combtn/item_0.png");
			((TextElement)lbName).text = "";
			((TextElement)lbCount).text = "";
		}
	}

	public static bool IsItemRare(EResourceType awardType, long awardId)
	{
		EQuality resourceQuality = GetResourceQuality(awardType, awardId);
		switch (awardType)
		{
		case EResourceType.Hero:
		case EResourceType.HeroFrag:
			return true;
		case EResourceType.Money:
			if (awardId == 2 || awardId == 8)
			{
				return true;
			}
			break;
		}
		if (awardType == EResourceType.Equip && resourceQuality > EQuality.Purple)
		{
			return true;
		}
		return resourceQuality > EQuality.Golden;
	}

	public static void RefreshResourceBg(EResourceType awardType, long awardId, VisualElement bg)
	{
		string resourceBgTex = GetResourceBgTex(GetResourceQuality(awardType, awardId));
		SetTextureBackground(bg, resourceBgTex);
	}

	public static void SetSpriteBackground(VisualElement el, string path)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Sprite val = zbAssetLoader.Instance.LoadResouce<Sprite>(path);
		if ((Object)(object)val != (Object)null && (Object)(object)val.texture != (Object)null)
		{
			el.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val.texture));
			el.MarkDirtyRepaint();
		}
	}

	public static void SetTextureBackground(VisualElement el, string path)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Texture2D val = zbAssetLoader.Instance.LoadResouce<Texture2D>(path);
		if ((Object)(object)val != (Object)null)
		{
			el.style.backgroundImage = new StyleBackground(Background.FromTexture2D(val));
			el.MarkDirtyRepaint();
		}
	}

	public static EQuality GetResourceQuality(EResourceType eResourceType, long id)
	{
		EQuality result = EQuality.Golden;
		switch (eResourceType)
		{
		case EResourceType.Money:
		{
			zbMoneyCsvRecord recordByKey4 = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey((int)id);
			if (recordByKey4 != null)
			{
				result = (EQuality)recordByKey4.quality;
			}
			break;
		}
		case EResourceType.Prop:
		{
			zbPropCsvRecord recordByKey8 = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)id);
			if (recordByKey8 != null)
			{
				result = (EQuality)recordByKey8.quality;
			}
			break;
		}
		case EResourceType.Item:
		{
			zbItemCsvRecord recordByKey2 = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)id);
			if (recordByKey2 != null)
			{
				result = (EQuality)recordByKey2.quality;
			}
			break;
		}
		case EResourceType.Equip:
		{
			zbEquipCsvRecord recordByKey6 = zbDataConfig.Instance.equipCsvTable.GetRecordByKey((int)id);
			if (recordByKey6 != null)
			{
				result = (EQuality)recordByKey6.quality;
			}
			break;
		}
		case EResourceType.Hero:
		case EResourceType.HeroFrag:
		{
			zbHeroCsvRecord recordByKey7 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)id);
			if (recordByKey7 != null)
			{
				result = ((recordByKey7.rarity != 10) ? ((EQuality)(recordByKey7.rarity + 2)) : EQuality.Blue);
			}
			break;
		}
		case EResourceType.Gem:
		{
			zbGemCsvRecord recordByKey5 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey((int)id);
			if (recordByKey5 != null)
			{
				result = (EQuality)recordByKey5.quality;
			}
			break;
		}
		case EResourceType.Box:
		{
			zbBoxCsvRecord recordByKey3 = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)id);
			if (recordByKey3 != null)
			{
				result = (EQuality)recordByKey3.quality;
			}
			break;
		}
		case EResourceType.Gene:
		{
			zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey((int)id);
			if (recordByKey != null)
			{
				result = (EQuality)recordByKey.quality;
			}
			break;
		}
		}
		return result;
	}

	public static stResorceIconInfo GetResorceIconPath(EResourceType eResorceType, long id)
	{
		stResorceIconInfo result = default(stResorceIconInfo);
		switch (eResorceType)
		{
		case EResourceType.Money:
		{
			zbMoneyCsvRecord recordByKey3 = zbDataConfig.Instance.moneyCsvTable.GetRecordByKey((int)id);
			if (recordByKey3 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey3.icon;
			result.strName = recordByKey3.name;
			result.strDesc = recordByKey3.desc;
			break;
		}
		case EResourceType.Prop:
		{
			zbPropCsvRecord recordByKey2 = zbDataConfig.Instance.propCsvTable.GetRecordByKey((int)id);
			if (recordByKey2 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey2.icon;
			result.strName = recordByKey2.name;
			result.strDesc = recordByKey2.desc;
			break;
		}
		case EResourceType.Item:
		{
			zbItemCsvRecord recordByKey5 = zbDataConfig.Instance.itemCsvTable.GetRecordByKey((int)id);
			if (recordByKey5 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey5.icon;
			result.strName = recordByKey5.name;
			result.strDesc = recordByKey5.desc;
			break;
		}
		case EResourceType.Equip:
		{
			zbEquipCsvRecord recordByKey11 = zbDataConfig.Instance.equipCsvTable.GetRecordByKey((int)id);
			if (recordByKey11 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey11.icon;
			result.strName = recordByKey11.name;
			result.strDesc = recordByKey11.desc;
			break;
		}
		case EResourceType.Hero:
		case EResourceType.HeroFrag:
		{
			zbHeroCsvRecord recordByKey6 = zbDataConfig.Instance.heroCsvTable.GetRecordByKey((int)id);
			if (recordByKey6 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			zbHeroAvatarCsvRecord recordByKey7 = zbDataConfig.Instance.heroAvatarCsvTable.GetRecordByKey(recordByKey6.avatarId);
			if (recordByKey7 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				result.strName = recordByKey6.name;
				return result;
			}
			result.strIconPath = recordByKey7.heroItemIcon;
			result.strName = recordByKey6.name;
			zbSkillCsvRecord recordByKey8 = zbDataConfig.Instance.skillCsvTable.GetRecordByKey(recordByKey6.skillId);
			result.strDesc = recordByKey8.desc;
			break;
		}
		case EResourceType.Gem:
		{
			zbGemCsvRecord recordByKey9 = zbDataConfig.Instance.gemCsvTable.GetRecordByKey((int)id);
			if (recordByKey9 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey9.icon;
			result.strName = recordByKey9.name;
			zbModCsvRecord recordByKey10 = zbDataConfig.Instance.modCsvTable.GetRecordByKey(recordByKey9.mod);
			if (recordByKey10 != null)
			{
				result.strDesc = recordByKey10.desc;
			}
			else
			{
				result.strDesc = "";
			}
			break;
		}
		case EResourceType.Box:
		{
			zbBoxCsvRecord recordByKey4 = zbDataConfig.Instance.boxCsvTable.GetRecordByKey((int)id);
			if (recordByKey4 == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey4.icon;
			result.strName = recordByKey4.name;
			result.strDesc = recordByKey4.desc;
			break;
		}
		case EResourceType.Gene:
		{
			zbGeneCsvRecord recordByKey = zbDataConfig.Instance.geneCsvTable.GetRecordByKey((int)id);
			if (recordByKey == null)
			{
				result.strIconPath = "ui/image/common/transparent.png";
				return result;
			}
			result.strIconPath = recordByKey.icon;
			result.strName = recordByKey.name;
			result.strDesc = recordByKey.desc;
			break;
		}
		}
		return result;
	}

	public static long GetResorceCount(EResourceType eResorceType, long tid)
	{
		long result = 0L;
		switch (eResorceType)
		{
		case EResourceType.Money:
			foreach (ResourceDetail money in zbServerData.sGameData.MoneyList)
			{
				if (money.Tid == tid && money.Count > 0)
				{
					return money.Count;
				}
			}
			break;
		case EResourceType.Prop:
			foreach (PropDetail prop in zbServerData.sGameData.PropList)
			{
				if (prop.Tid == tid && prop.Count > 0)
				{
					return prop.Count;
				}
			}
			break;
		case EResourceType.Item:
			foreach (ResourceDetail item in zbServerData.sGameData.ItemList)
			{
				if (item.Tid == tid && item.Count > 0)
				{
					return item.Count;
				}
			}
			break;
		case EResourceType.Gem:
			foreach (ResourceDetail gem in zbServerData.sGameData.GemList)
			{
				if (gem.Tid == tid && gem.Count > 0)
				{
					return gem.Count;
				}
			}
			break;
		case EResourceType.Gene:
			foreach (ResourceDetail gene in zbServerData.sGameData.GeneList)
			{
				if (gene.Tid == tid && gene.Count > 0)
				{
					return gene.Count;
				}
			}
			break;
		}
		return result;
	}

	public static string GetResourceFrameTex(EQuality quality)
	{
		return EResourceQualityFrame[(int)(quality - 1)];
	}

	public static string GetResourceBgTex(EQuality quality)
	{
		return EResourceQualityBg[(int)(quality - 1)];
	}

	public static string GetHeroFrameTex(EQuality quality)
	{
		return EHeroQualityBg[(int)(quality - 1)];
	}

	public static string GetShopFrameTex(EQuality quality)
	{
		return EShopQualityBg[(int)(quality - 1)];
	}

	public static string GetShopVipFrameTex(EQuality quality)
	{
		return EShopVipQualityBg[(int)(quality - 1)];
	}

	public static Color GetColor(EQuality quality)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return EColor[(int)(quality - 1)];
	}

	public static string GetPlayerHeadTex(int id)
	{
		zbPlayerHeadCsvRecord recordByKey = zbDataConfig.Instance.playerHeadCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.icon;
		}
		return "ui/image/icon/head/1.png";
	}

	public static string GetPlayerHeadFrameTex(int id)
	{
		zbPlayerHeadFrameCsvRecord recordByKey = zbDataConfig.Instance.playerHeadFrameCsvTable.GetRecordByKey(id);
		if (recordByKey != null)
		{
			return recordByKey.icon;
		}
		return "ui/image/icon/headframe/frame_1.png";
	}

	public static string GetHeroRarityIconPath(ERarityType rarity)
	{
		return $"ui/image/icon/herotag/rarity{(int)rarity}.png";
	}

	public static string GetSkillTypeIconPath(zbHeroCsvRecord heroRecord, ERarityType heroRarity = ERarityType.EmptyValue)
	{
		zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey(heroRecord.type);
		if (recordByKey != null)
		{
			ERarityType eRarityType = ((heroRarity > ERarityType.EmptyValue) ? heroRarity : ((ERarityType)heroRecord.rarity));
			return ((heroRecord.type == 1) ? ERarityType.Ss : eRarityType) switch
			{
				ERarityType.A => recordByKey.icon2, 
				ERarityType.S => recordByKey.icon3, 
				ERarityType.Ss => recordByKey.icon4, 
				_ => recordByKey.icon1, 
			};
		}
		return "";
	}

	public static string GetSkillTypeIconPathByType(ESkillType skillType)
	{
		zbSkillTypeCsvRecord recordByKey = zbDataConfig.Instance.skillTypeCsvTable.GetRecordByKey((int)skillType);
		if (recordByKey != null)
		{
			return (int)((skillType == ESkillType.Qiang) ? ERarityType.Ss : zbSelf.Instance.heroHandler.GetCurrentHeroRarityTypeBySkillType(skillType)) switch
			{
				2 => recordByKey.icon2, 
				3 => recordByKey.icon3, 
				4 => recordByKey.icon4, 
				_ => recordByKey.icon1, 
			};
		}
		return "";
	}

	public static long GetPropCountByType(int type)
	{
		foreach (PropDetail prop in zbServerData.sGameData.PropList)
		{
			if (prop.Tid == type)
			{
				return prop.Count;
			}
		}
		return 0L;
	}

	public static string GetMainLevelName(int id)
	{
		return $"Day {id}";
	}

	public static string GetEliteLevelName(int id)
	{
		return $"Night {id}";
	}

	public static string GetLegendaryLevelName(int id)
	{
		return zbUtil.Language("JinJiJiuYuan") + "-" + string.Format(zbUtil.Language("lang.10842"), id);
	}

	static zbResourceHelper()
	{
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0218: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		EResourceQualityFrame = new string[8] { "ui/image/frame/frame_1.png", "ui/image/frame/frame_2.png", "ui/image/frame/frame_3.png", "ui/image/frame/frame_4.png", "ui/image/frame/frame_5.png", "ui/image/frame/frame_6.png", "ui/image/frame/frame_7.png", "ui/image/frame/frame_8.png" };
		EResourceQualityBg = new string[7] { "ui/image/frame/tip_frame_1.png", "ui/image/frame/tip_frame_2.png", "ui/image/frame/tip_frame_3.png", "ui/image/frame/tip_frame_4.png", "ui/image/frame/tip_frame_5.png", "ui/image/frame/tip_frame_6.png", "ui/image/frame/tip_frame_7.png" };
		EHeroQualityBg = new string[7] { "ui/image/frame/hero_frame_1.png", "ui/image/frame/hero_frame_2.png", "ui/image/frame/hero_frame_3.png", "ui/image/frame/hero_frame_4.png", "ui/image/frame/hero_frame_5.png", "ui/image/frame/hero_frame_6.png", "ui/image/frame/hero_frame_7.png" };
		EShopQualityBg = new string[7] { "ui/image/frame/shop_bg_1.png", "ui/image/frame/shop_bg_2.png", "ui/image/frame/shop_bg_3.png", "ui/image/frame/shop_bg_4.png", "ui/image/frame/shop_bg_5.png", "ui/image/frame/shop_bg_6.png", "ui/image/frame/shop_bg_7.png" };
		EShopVipQualityBg = new string[7] { "ui/image/frame/vipshop_1.png", "ui/image/frame/vipshop_2.png", "ui/image/frame/vipshop_3.png", "ui/image/frame/vipshop_4.png", "ui/image/frame/vipshop_5.png", "ui/image/frame/vipshop_6.png", "ui/image/frame/vipshop_7.png" };
		EColor = (Color[])(object)new Color[8]
		{
			new Color(1f, 1f, 1f, 1f),
			new Color(0.2f, 1f, 0.2f, 1f),
			new Color(0.2f, 0.6f, 1f, 1f),
			new Color(0.7f, 0.4f, 1f, 1f),
			new Color(1f, 0.84f, 0f, 1f),
			new Color(1f, 0.3f, 0.3f, 1f),
			new Color(1f, 0.55f, 0f, 1f),
			new Color(1f, 0f, 1f, 1f)
		};
	}
}
