using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillDef : CTScriptableObject, IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>, IAssetIcon
{
	public enum SkillPropertyType
	{
		None = 0,
		CastDurability = 1,
		BattleDurability = 2,
		EffectDurability = 3,
		CastCoolDown = 101,
		SkillInternalValue = 200,
		Strength = 201,
		ActivateWithCoinCost = 301,
		ActivateWithCrystalCost = 302,
		ReplaceToActivate = 10001,
		LightActivate = 10002,
		InitialActivate = 10003,
		ReplaceRecycle = 10004,
		CastOncePerBattle = 10005
	}

	public class SkillProperty
	{
		public SkillPropertyType type;

		public int value;
	}

	public class SkillArchetype
	{
		public QualityLevel qualityLevel;

		public List<SkillProperty> properties;

		public SkillPropertyType ArchDurabilityType => properties.FirstOrDefault((SkillProperty p) => p.type >= SkillPropertyType.CastDurability && p.type < SkillPropertyType.CastCoolDown)?.type ?? SkillPropertyType.None;

		public SkillPropertyType ArchActivateCostType => properties.FirstOrDefault((SkillProperty p) => p.type >= SkillPropertyType.ActivateWithCoinCost && p.type <= (SkillPropertyType)401)?.type ?? SkillPropertyType.None;

		public bool WithDurability => ArchDurabilityType != SkillPropertyType.None;

		public bool WithActivateCost => ArchActivateCostType != SkillPropertyType.None;

		public int? GetPropValue(SkillPropertyType type)
		{
			return properties.Find((SkillProperty x) => x.type == type)?.value;
		}

		public bool GetPropFlag(SkillPropertyType type)
		{
			SkillProperty skillProperty = properties.Find((SkillProperty x) => x.type == type);
			if (skillProperty != null)
			{
				return skillProperty.value > 0;
			}
			return false;
		}

		public int? GetActivateCostValue()
		{
			if (GetPropValue(SkillPropertyType.ActivateWithCoinCost).HasValue)
			{
				return GetPropValue(SkillPropertyType.ActivateWithCoinCost);
			}
			if (GetPropValue(SkillPropertyType.ActivateWithCrystalCost).HasValue)
			{
				return GetPropValue(SkillPropertyType.ActivateWithCrystalCost);
			}
			return null;
		}
	}

	[SerializeField]
	[CTSOAssetName]
	[LocalizeText("护符名字（翻译时应当尽可能简短，最好不超过2个单词，可以适当传神意译）")]
	public string skillName;

	[SerializeField]
	private Sprite icon;

	[SerializeField]
	[LocalizeText("护符描述")]
	[TextArea]
	public string description;

	[SerializeField]
	public string storyText;

	[SerializeField]
	public List<SkillArchetype> skillArchetypes;

	[SerializeField]
	private MatterDef attachLootMatterOverride;

	[SerializeField]
	public CharacterClassDef attachCharacterClass;

	[SerializeField]
	public List<Ability> abilities;

	[SerializeField]
	public SkillSpell skillSpell;

	public override string CTAssetIDPrepend => "SK";

	public MatterDef attachLootMatter
	{
		get
		{
			if (!(attachLootMatterOverride != null))
			{
				return attachCharacterClass?.classMatter;
			}
			return attachLootMatterOverride;
		}
	}

	public int BaseArchTypeIndex => skillArchetypes.IndexOf(BaseQualityArchType);

	public SkillArchetype BaseQualityArchType => skillArchetypes.MinBy((SkillArchetype s) => (int)s.qualityLevel);

	public SkillArchetype MaxQualityArchType => skillArchetypes.MaxBy((SkillArchetype s) => (int)s.qualityLevel);

	public QualityLevel MaxQualityLevel
	{
		get
		{
			if (MaxQualityArchType == null)
			{
				return QualityLevel.Common;
			}
			return MaxQualityArchType.qualityLevel;
		}
	}

	public QualityLevel BaseQualityLevel
	{
		get
		{
			if (BaseQualityArchType == null)
			{
				return QualityLevel.Common;
			}
			return BaseQualityArchType.qualityLevel;
		}
	}

	public Sprite Icon
	{
		get
		{
			if (!(icon == null))
			{
				return icon;
			}
			return ScriptableObjectSingleton<GameDataManager>.instance.skillPlaceHolderIcon;
		}
		set
		{
			icon = value;
		}
	}

	public Sprite IconAsset => icon;

	public IEnumerable<(SkillDef, SkillArchetype)> AllSkillArchetypeTuple => skillArchetypes.Select((SkillArchetype arch) => (this, arch: arch));

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public PlayerCharacterDef GetAttachCharacer(AssetCatalog catalog)
	{
		return catalog.characterDefCatalog.assets.FirstOrDefault((PlayerCharacterDef t) => t.innateSkill.Contains(this));
	}

	public new List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (skillSpell != null)
		{
			list.Add(skillSpell.targetRequirement);
			list.Add(skillSpell.castableCheck);
			list.Add(skillSpell.castEffect);
		}
		foreach (Ability item in abilities.OrEmptyIfNull())
		{
			list.Add(item);
		}
		return list;
	}

	public static bool IsModifyValuePropertyType(SkillPropertyType type)
	{
		return type < (SkillPropertyType)1000;
	}
}
