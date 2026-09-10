public class PresetSkillSlot
{
	[SaveField(null, null)]
	public int index;

	[SaveField(null, null)]
	public ProfileSkill skill;

	[SaveField(null, null)]
	public SkillWordDef slotWord;

	[SaveField(null, null)]
	public bool characterInnateSkillSlot;

	public int displayIndex => index + 1;

	public PresetSkillSlot()
	{
	}

	public PresetSkillSlot(int index, ProfileSkill skill, SkillWordDef slotWord, bool characterInnateSkillSlot)
	{
		this.index = index;
		this.skill = skill;
		this.slotWord = slotWord;
		this.characterInnateSkillSlot = characterInnateSkillSlot;
	}

	public void SetSkill(ProfileSkill skill)
	{
		this.skill = skill;
	}

	public GameSkill GetGameSkill()
	{
		if (skill == null)
		{
			return null;
		}
		return new GameSkill(this);
	}

	public bool IsUnLocked(CharacterGameInitConfig characterConfig)
	{
		return index < characterConfig.unlockedSkillPresetSlotCount;
	}
}
