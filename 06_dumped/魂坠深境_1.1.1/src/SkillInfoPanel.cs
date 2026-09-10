using UnityEngine;

public class SkillInfoPanel : CommonInfoPanelBase
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
		if (currentCommonInfoData == null)
		{
			skillInfoDisplay.presetSkillSlot = null;
			skillInfoDisplay.Unset();
		}
		else if (currentCommonInfoData.target is PresetSkillSlot presetSkillSlot)
		{
			skillInfoDisplay.presetSkillSlot = presetSkillSlot;
			if (presetSkillSlot.GetGameSkill() == null)
			{
				skillInfoDisplay.gameObject.SetActive(value: true);
				skillInfoDisplay.SetDirty();
			}
			else
			{
				skillInfoDisplay.Setup(presetSkillSlot.GetGameSkill());
			}
		}
		else if (currentCommonInfoData.target is GameSkill setupData)
		{
			skillInfoDisplay.presetSkillSlot = null;
			skillInfoDisplay.Setup(setupData);
		}
	}
}
