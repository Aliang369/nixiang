using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillWordDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IGameEventSubscriber, IAssetIcon
{
	public enum SkillWordType
	{
		None,
		Red,
		Green,
		Yellow,
		Purple,
		White
	}

	[CTSOAssetName]
	[LocalizeText("护符词条名称")]
	public string wordName;

	[LocalizeText("护符词条描述")]
	public string description;

	public bool isUnique;

	[HideInInspector]
	public Sprite icon;

	public QualityLevel quality;

	public SkillWordType skillWordType;

	public List<SkillPropertyModifier> skillPropertyModifiers;

	public List<Ability> abilities;

	public List<SkillDef.SkillPropertyType> attachNeedProperties;

	public SkillConstraint attachConstraint;

	public override string CTAssetIDPrepend => "SKW";

	public Sprite Icon => ScriptableObjectSingleton<SkillGemNamingConfig>.instance?.GetWordTypeInfo(skillWordType)?.wordIcon ?? icon ?? ScriptableObjectSingleton<GameDataManager>.instance.skillWordPlaceHolderIcon;

	public string NameWithQualityColor => wordName.ColorText(ScriptableObjectSingleton<GameDataManager>.instance.GetQualityBaseColor(quality));

	public Sprite IconAsset => Icon;

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	List<IEffectNode> IChildren<IEffectNode>.GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		foreach (SkillPropertyModifier item in skillPropertyModifiers.OrEmptyIfNull())
		{
			list.Add(item);
		}
		foreach (Ability item2 in abilities.OrEmptyIfNull())
		{
			list.Add(item2);
		}
		return list;
	}

	public IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		foreach (Ability item in abilities.OrEmptyIfNull())
		{
			if (item == null)
			{
				continue;
			}
			foreach (GameEvent.Type item2 in item.GetAllSubscribeGameEventType())
			{
				yield return item2;
			}
		}
	}

	public bool CheckTriggerGameEventAble(GameEvent eve)
	{
		foreach (Ability item in abilities.OrEmptyIfNull())
		{
			if (item.CheckTriggerGameEventAble(eve))
			{
				return true;
			}
		}
		return false;
	}

	public void TriggerGameEvent(GameEvent eve)
	{
		foreach (Ability item in abilities.OrEmptyIfNull())
		{
			item.TriggerGameEvent(eve);
		}
	}

	public int GetFixAttachWordCost()
	{
		return ScriptableObjectSingleton<FortressDataManager>.instance.fixWordAttachWordQualityCostDic[quality];
	}

	public string GetFormatDescription(ApplicationStateInfo applicationState, out TextDataGenerator generator)
	{
		return TextDataPreset.FormatSkillWordDefDescription(this, applicationState, out generator);
	}

	public bool CheckAttachAble(ProfileSkill skill)
	{
		if (attachConstraint == null || attachConstraint.CheckSkillAccept(skill.GetGameSkill()) == true)
		{
			if (!attachNeedProperties.IsNullOrEmpty())
			{
				return attachNeedProperties.Any((SkillDef.SkillPropertyType p) => skill.SkillArchetype.GetPropValue(p).HasValue);
			}
			return true;
		}
		return false;
	}
}
