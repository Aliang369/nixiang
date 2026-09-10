using System.Collections.Generic;
using System.Linq;

public class ProfileSkillPreset
{
	[SaveField(null, null)]
	public int index;

	[SaveField(null, null)]
	public List<PresetSkillSlot> configSkillSlots;

	[SaveField(null, null)]
	public PlayerCharacterState character;

	public List<PresetSkillSlot> innateSkillSlots
	{
		get
		{
			List<PresetSkillSlot> list = new List<PresetSkillSlot>();
			List<ProfileSkill> characterCurrentInnateProfileSkills = character.GetCharacterCurrentInnateProfileSkills();
			for (int i = 0; i < characterCurrentInnateProfileSkills.Count; i++)
			{
				list.Add(new PresetSkillSlot(configSkillSlots.Count + i, characterCurrentInnateProfileSkills[i], null, characterInnateSkillSlot: true));
			}
			return list;
		}
	}

	public List<PresetSkillSlot> AllSkillSlots => innateSkillSlots.Concat(configSkillSlots).ToList();

	public List<ProfileSkill> AllConfigSkills => (from s in AllSkillSlots
		where s.skill != null
		select s.skill).ToList();

	public string name => ScriptableObjectSingleton<TextDataPreset>.instance.term_skillPreset_index_arg0.FormatWith((index + 1).ToString());

	private IEnumerable<SkillDef> CurrentInnateSkillDefinitions
	{
		get
		{
			if (character?.def?.innateSkill == null || character.unlockedTalents == null || !character.CharacterInnateSkillUnlocked)
			{
				return Enumerable.Empty<SkillDef>();
			}
			return character.def.innateSkill.Where((SkillDef skillDef) => skillDef != null);
		}
	}

	public bool ContainsSkillDefinition(SkillDef skillDef, PresetSkillSlot exceptSlot = null)
	{
		if (skillDef == null)
		{
			return false;
		}
		if (CurrentInnateSkillDefinitions.Contains(skillDef))
		{
			return true;
		}
		if (configSkillSlots != null)
		{
			return configSkillSlots.Any((PresetSkillSlot slot) => slot != null && slot != exceptSlot && slot.skill?.def == skillDef);
		}
		return false;
	}

	public List<(PresetSkillSlot slot, ProfileSkill skill)> NormalizeDuplicateSkillDefinitions()
	{
		List<(PresetSkillSlot, ProfileSkill)> list = new List<(PresetSkillSlot, ProfileSkill)>();
		HashSet<SkillDef> hashSet = new HashSet<SkillDef>(CurrentInnateSkillDefinitions);
		if (configSkillSlots == null)
		{
			return list;
		}
		foreach (PresetSkillSlot item in from slot in configSkillSlots
			where slot != null
			orderby slot.index
			select slot)
		{
			ProfileSkill skill = item.skill;
			if (!(skill?.def == null) && !hashSet.Add(skill.def))
			{
				list.Add((item, skill));
				item.SetSkill(null);
			}
		}
		return list;
	}

	public ProfileSkillPreset()
	{
	}

	public ProfileSkillPreset(int index, PlayerCharacterState character)
	{
		this.index = index;
		this.character = character;
		configSkillSlots = new List<PresetSkillSlot>();
		int maxSkillSlotCount = this.character.def.MaxSkillSlotCount;
		int num = 0;
		for (int i = 0; i < maxSkillSlotCount; i++)
		{
			configSkillSlots.Add(new PresetSkillSlot(num++, null, null, characterInnateSkillSlot: false));
		}
	}
}
