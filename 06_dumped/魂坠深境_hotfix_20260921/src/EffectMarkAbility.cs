using System;
using System.Collections.Generic;
using UnityEngine;

public class EffectMarkAbility : Ability
{
	[Flags]
	public enum ShowType
	{
		None = 0,
		Mark = 1,
		Effect = 2,
		All = -1
	}

	public class EffectMarkInfo
	{
		public string description;

		public object source;

		public EffectMarkNFX nfx;

		public ShowType showType;

		public Sprite GetIcon()
		{
			if (source is Item item)
			{
				return item.Icon;
			}
			if (source is Influence influence)
			{
				return influence.def.Icon;
			}
			if (source is Rune rune)
			{
				return rune.def.Icon;
			}
			if (source is Enchant enchant)
			{
				return enchant.def.Icon;
			}
			if (source is GameSkill gameSkill)
			{
				return gameSkill.Icon;
			}
			if (source is SkillWord skillWord)
			{
				return skillWord.gameSkill.Icon;
			}
			if (source is Contract contract)
			{
				return contract.contractDef.icon;
			}
			return null;
		}

		public string GetSourceText()
		{
			return ScriptableObjectSingleton<TextDataPreset>.instance.term_effect_mark_from_arg0_type_arg1_object.FormatWith(GetTypeName(), GetName());
		}

		public string GetDescriptionText()
		{
			return description;
		}

		public string GetTypeName()
		{
			if (source is Item)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_item;
			}
			if (source is Influence)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_influence;
			}
			if (source is Rune)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_rune;
			}
			if (source is Enchant)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_enchant;
			}
			if (source is GameSkill)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill;
			}
			if (source is SkillWord)
			{
				return ScriptableObjectSingleton<TextDataPreset>.instance.term_skill;
			}
			return "";
		}

		public string GetName()
		{
			if (source is Item item)
			{
				return item.ItemName;
			}
			if (source is Influence influence)
			{
				return influence.def.InfluenceName;
			}
			if (source is Rune rune)
			{
				return rune.def.Name;
			}
			if (source is Enchant enchant)
			{
				return enchant.def.NameWithColor;
			}
			if (source is GameSkill gameSkill)
			{
				return gameSkill.Name;
			}
			if (source is SkillWord skillWord)
			{
				return skillWord.gameSkill.Name;
			}
			return "";
		}
	}

	public ShowType showType = ShowType.Mark;

	public EffectMarkNFX buffEffect;

	[LocalizeText("效果标记描述")]
	public string rawDescription;

	public GameEvent.EventBFTType targetOwner;

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.EffectMarkResolve;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.EffectMarkResolve)
		{
			return;
		}
		ItemContainer owner = eve.TargetItem.owner;
		if (owner != null && owner.type == ItemContainer.ContainerType.Table && eve.TargetItem.OwnerBft == eve.GetEventBFT(targetOwner) && CheckTriggerGameEventAble(eve))
		{
			object eveObj = eve.GetEveObj(GameEvent.EveObjType.Caster);
			string description = rawDescription;
			if (eveObj is IEffectObjectDisplay obj)
			{
				description = TextDataPreset.FormatEffectObjectDescription(obj, description, eve, out var _);
			}
			EffectMarkInfo item = new EffectMarkInfo
			{
				source = eveObj,
				description = description,
				nfx = buffEffect,
				showType = showType
			};
			eve.resolveInfo.effectMarkInfos.Add(item);
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			str = new NodeDisplayInfo.Element<string>(rawDescription)
		};
	}
}
