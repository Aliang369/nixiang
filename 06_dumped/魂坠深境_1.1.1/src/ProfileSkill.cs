using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProfileSkill : IEffectObjectDisplay, IEffectNode, ITextDisplay, IChildren<IEffectNode>
{
	public class SkillWordSlot
	{
		[SaveField(null, null)]
		public SkillWordDef word;
	}

	[SaveField(null, null)]
	public SkillDef def;

	[SaveField(null, null)]
	public int archetypeIndex;

	[SaveField(null, null)]
	public bool inspected;

	[SaveField(null, null)]
	public string profileSkillGUID;

	[SaveField(null, null)]
	public bool isFavorite;

	[SaveField(null, null)]
	public HexShape hexShape;

	[SaveField(null, null)]
	public List<SkillGem> skillGemsMounted;

	public int HexShapeSlotCount => (hexShape?.points?.Count).GetValueOrDefault();

	public int AttachedGemCount => skillGemsMounted?.Count ?? 0;

	public SkillDef.SkillArchetype SkillArchetype => def.skillArchetypes.ElementAtOrDefault(archetypeIndex) ?? def.skillArchetypes.First();

	public QualityLevel QualityLevel => SkillArchetype.qualityLevel;

	public bool IsDeleteAble => !isFavorite;

	public Sprite Icon => def.Icon;

	public string Name => def.skillName;

	private ProfileSkill()
	{
	}

	public ProfileSkill(SkillDef def)
		: this(def, def.BaseArchTypeIndex)
	{
	}

	public ProfileSkill(SkillDef def, SkillDef.SkillArchetype archetype, int level, List<SkillWordDef> initWords = null)
		: this(def, def.skillArchetypes.IndexOf(archetype))
	{
	}

	public ProfileSkill(SkillDef def, int archetypeIndex)
	{
		this.def = def;
		inspected = false;
		this.archetypeIndex = archetypeIndex;
		profileSkillGUID = Guid.NewGuid().ToString();
	}

	public NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public List<IEffectNode> GetChildren()
	{
		return def.GetChildren();
	}

	public void AddSkillWord(ApplicationStateInfo applicationState, SkillWordDef wordDef, SkillWordSlot slot)
	{
		slot.word = wordDef;
		applicationState.eventManager.addSkillWordEvent.TriggerEvent((this, wordDef));
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	public void RemoveSkillWord(ApplicationStateInfo applicationState, SkillWordSlot slot)
	{
		SkillWordDef word = slot.word;
		slot.word = null;
		applicationState.eventManager.removeSkillWordEvent.TriggerEvent((this, word));
		applicationState.savingManager.SetProfileDirtyToSave();
	}

	public List<MatterInfo> GetDeleteSkillGainMatter()
	{
		List<MatterInfo> list = new List<MatterInfo>();
		int num = ScriptableObjectSingleton<FortressDataManager>.instance.deleteSkillGainRecycleMatterQualityDic[QualityLevel];
		if (num > 0)
		{
			list.Add(new MatterInfo(ScriptableObjectSingleton<FortressDataManager>.instance.essenceCurrencyMatter, num));
		}
		return list;
	}

	public GameSkill GetGameSkill()
	{
		return new GameSkill(this);
	}

	public ProfileSkill GetBreakthroughPreview()
	{
		ProfileSkill profileSkill = new ProfileSkill(def, archetypeIndex);
		profileSkill.hexShape = hexShape;
		profileSkill.skillGemsMounted = skillGemsMounted;
		profileSkill.Breakthrough();
		return profileSkill;
	}

	public bool CanBreakthrough()
	{
		return def.skillArchetypes.Find((SkillDef.SkillArchetype a) => a.qualityLevel == QualityLevel + 1) != null;
	}

	public void Breakthrough()
	{
		if (CanBreakthrough())
		{
			SkillDef.SkillArchetype item = def.skillArchetypes.Find((SkillDef.SkillArchetype a) => a.qualityLevel == QualityLevel + 1);
			archetypeIndex = def.skillArchetypes.IndexOf(item);
		}
	}

	public void SetArchetypeIndex(QualityLevel qualityLevel)
	{
		SkillDef.SkillArchetype skillArchetype = def?.skillArchetypes?.Find((SkillDef.SkillArchetype a) => a.qualityLevel == qualityLevel);
		if (skillArchetype == null)
		{
			archetypeIndex = 0;
		}
		else
		{
			archetypeIndex = def.skillArchetypes.IndexOf(skillArchetype);
		}
	}

	public static List<(string, IComparer<ProfileSkill>)> GetAllProfileSkillSortMethod(Func<ProfileSkill, bool> selectable = null)
	{
		return new List<(string, IComparer<ProfileSkill>)>
		{
			new ValueTuple<string, IComparer<ProfileSkill>>(item2: Comparer<ProfileSkill>.Create(delegate(ProfileSkill a, ProfileSkill b)
			{
				int num = 0;
				if (selectable != null)
				{
					num = selectable(b).CompareTo(selectable(a));
					if (num != 0)
					{
						return num;
					}
				}
				num = b.QualityLevel.CompareTo(a.QualityLevel);
				if (num != 0)
				{
					return num;
				}
				num = b.def.skillName.CompareTo(a.def.skillName);
				if (num != 0)
				{
					return num;
				}
				num = a.archetypeIndex.CompareTo(b.archetypeIndex);
				return (num != 0) ? num : a.profileSkillGUID.CompareTo(b.profileSkillGUID);
			}), item1: ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_sort_by_quality),
			new ValueTuple<string, IComparer<ProfileSkill>>(item2: Comparer<ProfileSkill>.Create(delegate(ProfileSkill a, ProfileSkill b)
			{
				int num = 0;
				if (selectable != null)
				{
					num = selectable(b).CompareTo(selectable(a));
					if (num != 0)
					{
						return num;
					}
				}
				num = b.def.skillName.CompareTo(a.def.skillName);
				if (num != 0)
				{
					return num;
				}
				num = b.QualityLevel.CompareTo(a.QualityLevel);
				if (num != 0)
				{
					return num;
				}
				num = a.archetypeIndex.CompareTo(b.archetypeIndex);
				return (num != 0) ? num : a.profileSkillGUID.CompareTo(b.profileSkillGUID);
			}), item1: ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_sort_by_name),
			new ValueTuple<string, IComparer<ProfileSkill>>(item2: Comparer<ProfileSkill>.Create(delegate(ProfileSkill a, ProfileSkill b)
			{
				int num = 0;
				if (selectable != null)
				{
					num = selectable(b).CompareTo(selectable(a));
					if (num != 0)
					{
						return num;
					}
				}
				num = b.isFavorite.CompareTo(a.isFavorite);
				if (num != 0)
				{
					return num;
				}
				num = b.def.skillName.CompareTo(a.def.skillName);
				if (num != 0)
				{
					return num;
				}
				num = b.QualityLevel.CompareTo(a.QualityLevel);
				if (num != 0)
				{
					return num;
				}
				num = a.archetypeIndex.CompareTo(b.archetypeIndex);
				return (num != 0) ? num : a.profileSkillGUID.CompareTo(b.profileSkillGUID);
			}), item1: ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_sort_by_favorite),
			new ValueTuple<string, IComparer<ProfileSkill>>(item2: Comparer<ProfileSkill>.Create(delegate(ProfileSkill a, ProfileSkill b)
			{
				int num = 0;
				if (selectable != null)
				{
					num = selectable(b).CompareTo(selectable(a));
					if (num != 0)
					{
						return num;
					}
				}
				num = b.AttachedGemCount.CompareTo(a.AttachedGemCount);
				if (num != 0)
				{
					return num;
				}
				num = b.HexShapeSlotCount.CompareTo(a.HexShapeSlotCount);
				if (num != 0)
				{
					return num;
				}
				num = b.def.skillName.CompareTo(a.def.skillName);
				if (num != 0)
				{
					return num;
				}
				num = b.QualityLevel.CompareTo(a.QualityLevel);
				if (num != 0)
				{
					return num;
				}
				num = a.archetypeIndex.CompareTo(b.archetypeIndex);
				return (num != 0) ? num : a.profileSkillGUID.CompareTo(b.profileSkillGUID);
			}), item1: ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_sort_by_gem_slot)
		};
	}
}
