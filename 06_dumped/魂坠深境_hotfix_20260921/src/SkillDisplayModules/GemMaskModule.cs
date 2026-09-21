using UnityEngine;

namespace SkillDisplayModules;

public class GemMaskModule : SkillDisplay.DisplayModule
{
	[SerializeField]
	private GemSlotMark gemSlotMark;

	[SerializeField]
	private GameObject hasGemMark;

	public override void Init()
	{
		parent.InitUI(gemSlotMark);
	}

	public override void UpdateDisplay()
	{
		if (!CanShowGemSlotInfo() || base.gameSkill.profileSkill?.hexShape == null)
		{
			gemSlotMark.Unset();
			gemSlotMark.gameObject.SetActive(value: false);
			hasGemMark.SetActive(value: false);
		}
		else
		{
			gemSlotMark.Setup(base.gameSkill.profileSkill.hexShape);
			gemSlotMark.gameObject.SetActive(value: true);
			hasGemMark.SetActive(!base.gameSkill.profileSkill.skillGemsMounted.IsNullOrEmpty());
		}
	}

	private bool CanShowGemSlotInfo()
	{
		ApplicationStateInfo applicationStateInfo = UIDisplayModule.applicationState;
		if (applicationStateInfo == null)
		{
			return false;
		}
		return applicationStateInfo.globalVariableManager?.GetVariableFlag(ApplicationGlobalVariableManager.EnableSkillMountGem) == true;
	}
}
