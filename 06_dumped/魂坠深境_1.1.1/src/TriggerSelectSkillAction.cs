using System;

public class TriggerSelectSkillAction : ExecuteAction
{
	private readonly Action<GameSkill> confirmCallback;

	public TriggerSelectSkillAction(Action<GameSkill> confirmCallback = null)
	{
		this.confirmCallback = confirmCallback;
	}

	protected override void Update()
	{
		if (gameState.playerState.PreparableSkills.Count != 0)
		{
			gameState.UIManager.selectSkillPanel.Setup(new SelectSkillInfo
			{
				canCancel = true,
				confirmCallback = confirmCallback
			});
		}
	}
}
