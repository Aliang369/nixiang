using UnityEngine;

public class SkillUpgradeSelectButton : UIBase
{
	[SerializeField]
	private GameButton button;

	protected override void Init()
	{
		button.onClick.AddListener(UIBase.fortressState.uiManager.skill_GemPanel.skillUpgradePanel.ShowSelectPanel);
	}
}
