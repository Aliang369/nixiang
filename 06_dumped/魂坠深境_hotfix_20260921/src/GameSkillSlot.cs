public class GameSkillSlot : PresetSkillSlot
{
	[SaveField(null, null)]
	public GameSkill gameSkill;

	public bool withWordSlot => slotWord != null;

	public bool isTemp => !Bootstrap.applicationState.runningGameState.playerState.activeSkills_inSlot.Contains(this);

	public GameSkillSlot()
	{
	}

	public GameSkillSlot(PresetSkillSlot presetSlot)
		: base(presetSlot.index, presetSlot.skill, presetSlot.slotWord, presetSlot.characterInnateSkillSlot)
	{
		gameSkill = presetSlot.GetGameSkill();
	}

	public void SetSkill(GameSkill skill)
	{
		gameSkill = skill;
		base.skill = skill?.profileSkill;
	}
}
