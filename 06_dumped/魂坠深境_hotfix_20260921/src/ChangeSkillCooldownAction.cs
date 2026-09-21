public class ChangeSkillCooldownAction : ExecuteAction
{
	public int delta;

	public GameSkill skill;

	public ChangeSkillCooldownAction(GameSkill skill, int delta)
	{
		this.skill = skill;
		this.delta = delta;
	}

	protected override void Update()
	{
		skill.cooldown += delta;
		if (skill.cooldown < 0)
		{
			skill.cooldown = 0;
		}
		SkillDisplay skillDisplay = gameState.UIManager.playerBftUIDisplay.skillPanel.GetSkillDisplay(skill);
		if (skillDisplay != null)
		{
			gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.Block_coolDown, skillDisplay.transform, skillDisplay.transform.position);
		}
	}
}
