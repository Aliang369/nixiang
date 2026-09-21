public class ChangeSkillDurabilityAction : ExecuteAction
{
	public int delta;

	public GameSkill skill;

	public ChangeSkillDurabilityAction(GameSkill skill, int delta)
	{
		this.skill = skill;
		this.delta = delta;
	}

	protected override void Update()
	{
		if (skill.WithDurability)
		{
			skill.durability += delta;
			gameState.gameEventManager.TriggerSkillDurabilityChange(skill, delta, null);
			if (skill.durability <= 0)
			{
				gameState.gameEventManager.TriggerConsumeSkill(skill);
				gameState.gameEventManager.TriggerSkillDurabilityClean(skill);
				EnqueueAction(new ChangeSkillAction(skill, ChangeSkillAction.Type.Remove));
				EnqueueAction(new HintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_arg0_removed_for_durability_empty.FormatWith(skill.NameWithQualityAndUpgrade)));
			}
			else if (delta < 0)
			{
				EnqueueAction(new HintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_arg0_durability_decreased.FormatWith(skill.NameWithQualityAndUpgrade)));
			}
			else if (delta > 0)
			{
				EnqueueAction(new HintAction(ScriptableObjectSingleton<TextDataPreset>.instance.term_skill_arg0_durability_increased.FormatWith(skill.NameWithQualityAndUpgrade)));
			}
		}
	}
}
