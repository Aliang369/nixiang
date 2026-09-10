public class RefreshAllSkillAction : ExecuteAction
{
	protected override void Update()
	{
		foreach (GameSkill activatedSkill in gameState.playerState.ActivatedSkills)
		{
			if (activatedSkill.cooldown != 0)
			{
				activatedSkill.cooldown = 0;
				SkillDisplay skillDisplay = gameState.UIManager.playerBftUIDisplay.skillPanel.GetSkillDisplay(activatedSkill);
				if (skillDisplay != null)
				{
					gameState.applicationState.nfxManager.SpawnAndPlay(ScriptableObjectSingleton<VFXDataPreset>.instance.Block_coolDown, skillDisplay.transform, skillDisplay.transform.position);
				}
			}
		}
	}
}
