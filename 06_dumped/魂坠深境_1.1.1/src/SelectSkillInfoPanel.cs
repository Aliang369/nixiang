using UnityEngine;

public class SelectSkillInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private SkillInfoDisplay skillInfoDisplay;

	protected override void Init()
	{
		base.Init();
		InitUI(skillInfoDisplay);
	}

	protected override void UpdateInfo()
	{
		GameSkill gameSkill = null;
		if (currentCommonInfoData?.target is GameSkillSlot { gameSkill: not null } gameSkillSlot)
		{
			gameSkill = gameSkillSlot.gameSkill;
		}
		else if (currentCommonInfoData?.target is GameSkill gameSkill2)
		{
			gameSkill = gameSkill2;
		}
		else if (currentCommonInfoData?.target is ProfileSkill profileSkill)
		{
			gameSkill = new GameSkill(profileSkill);
		}
		if (gameSkill == null)
		{
			skillInfoDisplay.Unset();
		}
		else
		{
			skillInfoDisplay.Setup(gameSkill, currentCommonInfoData.appendData);
		}
	}
}
