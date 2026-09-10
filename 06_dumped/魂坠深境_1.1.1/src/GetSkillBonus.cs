using System.Collections.Generic;
using UnityEngine;

public class GetSkillBonus : BattleBonusObject
{
	public readonly GameSkill skill;

	public override object infoObject => skill;

	public override Sprite PreviewIcon => skill.Icon;

	public override string previewText => skill.Name;

	private GetSkillBonus()
	{
	}

	public GetSkillBonus(GameSkill skill)
	{
		this.skill = skill;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			skills = new List<GameSkill> { skill }
		}));
		ConfirmGotBonus();
	}
}
