using UnityEngine;

public class SkillGemInfoPanel : CommonInfoPanelBase
{
	[SerializeField]
	private SkillGemInfoDisplay skillGemInfoDisplay;

	protected override void Init()
	{
		base.Init();
		InitUI(skillGemInfoDisplay);
		skillGemInfoDisplay.Unset();
	}

	protected override void UpdateInfo()
	{
		if (!(currentCommonInfoData?.target is SkillGem setupData))
		{
			skillGemInfoDisplay.Unset();
		}
		else
		{
			skillGemInfoDisplay.Setup(setupData);
		}
	}
}
