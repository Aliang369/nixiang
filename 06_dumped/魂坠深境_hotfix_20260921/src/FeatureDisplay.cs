using System;
using System.Collections.Generic;
using Common.UI;
using ContainerOfSoul.Display.Fortress.SkillOverview;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeatureDisplay : CellUI<FeatureDisplay.DisplayInfo>, IDisplayModulesController
{
	public class DisplayInfo
	{
		public object target;

		public Sprite icon;

		public string name;

		public string description;

		public string iconStackText;

		public TextDataGenerator generator;

		public bool hideTypeName;

		public string typeNameOverride;

		public string statusText;
	}

	[SerializeField]
	private Image icon;

	[SerializeField]
	private TextMeshProUGUI title;

	[SerializeField]
	private TextMeshProUGUI description;

	[SerializeField]
	private TextMeshProUGUI statusText;

	[NonSerialized]
	public Func<DisplayInfo, bool> clickShowInfoOverride;

	[NonSerialized]
	private TextLinkObjectDisplayer textLinkObjectDisplayer;

	[NonSerialized]
	private Vector2 titleDefaultAnchoredPosition;

	protected override void Init()
	{
		textLinkObjectDisplayer = new CommonTextLinkObjectDisplayer(CellUI<DisplayInfo>.applicationState, description);
		icon.preserveAspect = true;
		titleDefaultAnchoredPosition = title.rectTransform.anchoredPosition;
		InputModule module = GetModule<InputModule>();
		module.onHandleInput = (Action<InputModuleBase>)Delegate.Combine(module.onHandleInput, (Action<InputModuleBase>)delegate(InputModuleBase InputModule)
		{
			if (InputModule.pointerClicked)
			{
				Func<DisplayInfo, bool> func = clickShowInfoOverride;
				if (func == null || !func(base.setupData))
				{
					((ICommonInfoPanel)UIHelper.GetDefaultCommonInfoPanel()).ShowCommonInfo(new CommonInfoData(base.setupData.target, CommonInfoData.ControlType.Click, base.rectTransform, false));
				}
			}
		});
	}

	protected override void OnSetup(DisplayInfo _)
	{
		if (base.setupData.icon != null)
		{
			icon.gameObject.SetActive(value: true);
			icon.sprite = base.setupData.icon;
		}
		else
		{
			icon.gameObject.SetActive(value: false);
		}
		title.text = base.setupData.name;
		string text = (string.IsNullOrEmpty(base.setupData.typeNameOverride) ? GetTypeName(base.setupData.target) : base.setupData.typeNameOverride);
		bool flag = base.setupData.hideTypeName || string.IsNullOrEmpty(text);
		description.text = text;
		description.gameObject.SetActive(!flag);
		title.rectTransform.anchoredPosition = (flag ? new Vector2(titleDefaultAnchoredPosition.x, (0f - base.rectTransform.rect.height) * 0.5f) : titleDefaultAnchoredPosition);
		if (statusText != null)
		{
			statusText.text = base.setupData.statusText;
			statusText.gameObject.SetActive(!string.IsNullOrEmpty(base.setupData.statusText));
		}
		textLinkObjectDisplayer.SetGenerator(base.setupData.generator);
	}

	public static List<DisplayInfo> GetDisplayInfos(TextDataGenerator generator)
	{
		List<DisplayInfo> list = new List<DisplayInfo>();
		for (int i = 0; i < generator.refLinkObjects.Count; i++)
		{
			DisplayInfo displayInfo = GetDisplayInfo(generator.refLinkObjects[i]);
			if (displayInfo != null)
			{
				list.Add(displayInfo);
			}
		}
		return list;
	}

	public static DisplayInfo GetDisplayInfo(object target)
	{
		if (target is DisplayInfo result)
		{
			return result;
		}
		if (target is ItemDefinition def)
		{
			return GetItem(new Item(def));
		}
		if (target is Item item)
		{
			return GetItem(item);
		}
		if (target is KeywordDef keywordDef)
		{
			return GetKeyword(keywordDef);
		}
		if (target is KeywordInfo keywordInfo)
		{
			return GetKeyword(keywordInfo);
		}
		if (target is Enchant enchant)
		{
			return GetEnchant(enchant);
		}
		if (target is EnchantDef enchantDef)
		{
			return GetEnchantDef(enchantDef);
		}
		if (target is RuneDef def2)
		{
			return GetRune(new Rune(def2));
		}
		if (target is Rune rune)
		{
			return GetRune(rune);
		}
		if (target is GameSkill skill)
		{
			return GetSkill(skill);
		}
		if (target is SkillWord skillWord)
		{
			return GetSkillWord(skillWord);
		}
		if (target is SkillWordDef skillWordDef)
		{
			return GetSkillWordDef(skillWordDef);
		}
		if (target is SkillDef def3)
		{
			return GetSkill(new GameSkill(new ProfileSkill(def3, 0)));
		}
		if (target is ProfileSkillPresetSlotCell.Info info)
		{
			if (info.slot?.skill != null)
			{
				return GetSkill(new GameSkill(info.slot.skill));
			}
			return null;
		}
		if (target is PresetSkillSlot presetSkillSlot)
		{
			if (presetSkillSlot.skill == null)
			{
				return null;
			}
			return GetSkill(new GameSkill(presetSkillSlot.skill));
		}
		if (target is Influence influence)
		{
			return GetSkill(influence);
		}
		if (target is Stance stance)
		{
			return GetStance(stance);
		}
		if (target is StanceDef stanceDef)
		{
			return GetStanceDef(stanceDef);
		}
		if (target is CharacterMastery mastery)
		{
			return GetCharacterMastery(mastery);
		}
		if (target is TalentDef talentDef)
		{
			return GetTalent(talentDef);
		}
		if (target is FeatureInfoData featureInfoData)
		{
			return GetDisplayInfo(featureInfoData.featureObject ?? featureInfoData.target);
		}
		if (target is BattleBonusObject battleBonusObject)
		{
			return new DisplayInfo
			{
				target = battleBonusObject,
				icon = battleBonusObject.PreviewIcon,
				name = battleBonusObject.previewText,
				description = battleBonusObject.description
			};
		}
		if (target is BonusLocationBonus bonusLocationBonus)
		{
			if (bonusLocationBonus.bonus.infoObject == null)
			{
				return null;
			}
			return GetDisplayInfo(bonusLocationBonus.bonus.infoObject);
		}
		TraceLog.Error($"该类型{target.GetType()}不支持FeatureDisplay显示");
		return null;
	}

	private static DisplayInfo GetItem(Item item)
	{
		return new DisplayInfo
		{
			target = item,
			icon = item.GetSkinIcon(CellUI<DisplayInfo>.applicationState),
			name = item.ItemName,
			description = item.GetStoryDescription()
		};
	}

	private static DisplayInfo GetKeyword(KeywordInfo keywordInfo)
	{
		TextDataPreset.FormatKeywordDescription(CellUI<DisplayInfo>.applicationState, keywordInfo.keywordDef, out var generator);
		return new DisplayInfo
		{
			target = keywordInfo.keywordDef,
			name = keywordInfo.keywordDef.keywordName,
			description = keywordInfo.keywordDef.displayNode.GetKeywordDescription(CellUI<DisplayInfo>.applicationState, keywordInfo),
			generator = generator
		};
	}

	private static DisplayInfo GetKeyword(KeywordDef keywordDef)
	{
		TextDataPreset.FormatKeywordDescription(CellUI<DisplayInfo>.applicationState, keywordDef, out var generator);
		return new DisplayInfo
		{
			target = keywordDef,
			name = keywordDef.keywordName,
			description = keywordDef.displayNode.GetKeywordDescription(CellUI<DisplayInfo>.applicationState, keywordDef.displayNode.GenerateKeywordInfo(keywordDef, null, null)),
			generator = generator
		};
	}

	private static DisplayInfo GetEnchant(Enchant enchant)
	{
		DisplayInfo displayInfo = new DisplayInfo
		{
			target = enchant,
			icon = enchant.def.Icon,
			name = enchant.def.NameWithColor,
			description = enchant.GetDescription(CellUI<DisplayInfo>.applicationState, out var generator)
		};
		if (enchant.def.stacking)
		{
			displayInfo.iconStackText = $"{enchant.value}";
		}
		displayInfo.generator = generator;
		return displayInfo;
	}

	private static DisplayInfo GetEnchantDef(EnchantDef enchantDef)
	{
		Enchant enchant = new Enchant(enchantDef);
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = enchantDef,
			icon = enchant.def.Icon,
			name = enchantDef.NameWithColor,
			description = enchant.GetDescription(CellUI<DisplayInfo>.applicationState, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetRune(Rune rune)
	{
		DisplayInfo displayInfo = new DisplayInfo
		{
			target = rune,
			icon = rune.def.Icon,
			name = (rune.GetNameWithValue() ?? ""),
			description = rune.GetDescription(CellUI<DisplayInfo>.applicationState, out var generator)
		};
		displayInfo.generator = generator;
		if (rune.def.stacking)
		{
			displayInfo.iconStackText = $"{rune.value}";
		}
		return displayInfo;
	}

	private static DisplayInfo GetSkill(GameSkill skill)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = skill,
			icon = skill.skillDef.Icon,
			name = skill.NameWithQualityColor,
			description = TextDataPreset.FormatSkillDescription(skill, CellUI<DisplayInfo>.applicationState, isBattleDisplay: false, out generator),
			hideTypeName = true,
			generator = generator
		};
	}

	private static DisplayInfo GetSkillWord(SkillWord skillWord)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = skillWord,
			icon = skillWord.Icon,
			name = skillWord.NameWithQualityColor,
			description = skillWord.GetFormatDescription(CellUI<DisplayInfo>.applicationState, null, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetSkillWordDef(SkillWordDef skillWordDef)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = skillWordDef,
			icon = skillWordDef.Icon,
			name = skillWordDef.NameWithQualityColor,
			description = skillWordDef.GetFormatDescription(CellUI<DisplayInfo>.applicationState, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetSkill(Influence influence)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = influence,
			icon = influence.def.Icon,
			name = influence.def.InfluenceName,
			description = influence.GetInfluenceDescription(CellUI<DisplayInfo>.applicationState, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetStance(Stance stance)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = stance,
			name = stance.def.stanceName,
			description = TextDataPreset.FormatStanceDescription(CellUI<DisplayInfo>.gameState, stance, out generator),
			iconStackText = $"{stance.value}",
			generator = generator
		};
	}

	private static DisplayInfo GetStanceDef(StanceDef stanceDef)
	{
		Stance stance = new Stance(stanceDef, 0, CellUI<DisplayInfo>.gameState.playerState.bft);
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = stanceDef,
			name = stanceDef.stanceName,
			description = TextDataPreset.FormatStanceDescription(CellUI<DisplayInfo>.gameState, stance, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetCharacterMastery(CharacterMastery mastery)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = mastery,
			icon = mastery.icon,
			name = mastery.masteryName,
			description = mastery.GetDescription(CellUI<DisplayInfo>.applicationState, out generator),
			generator = generator
		};
	}

	private static DisplayInfo GetTalent(TalentDef talentDef)
	{
		TextDataGenerator generator;
		return new DisplayInfo
		{
			target = talentDef,
			icon = talentDef.icon,
			name = talentDef.talentDisplayName,
			description = talentDef.GetDisplayDescription(CellUI<DisplayInfo>.applicationState, null, out generator),
			generator = generator
		};
	}

	private static string GetTypeName(object target)
	{
		if (target is Item)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_item;
		}
		if (target is KeywordDef)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_keyword;
		}
		if (target is Enchant || target is EnchantDef)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_enchant;
		}
		if (target is Rune || target is RuneDef)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_rune;
		}
		if (target is GameSkill)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill;
		}
		if (target is SkillWord || target is SkillWordDef)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_skillWord;
		}
		if (target is Influence)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_influence;
		}
		if (target is Stance || target is StanceDef)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_stance;
		}
		if (target is CharacterMastery)
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_mastery;
		}
		return "";
	}
}
