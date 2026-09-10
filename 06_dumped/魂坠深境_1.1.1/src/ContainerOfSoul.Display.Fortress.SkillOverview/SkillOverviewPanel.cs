using UnityEngine;

namespace ContainerOfSoul.Display.Fortress.SkillOverview;

public class SkillOverviewPanel : FortressUIManager.SubPanel
{
	[SerializeField]
	public SkillStoragePanel skillStoragePanel;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	private RectTransform helpHint;

	[SerializeField]
	private GameButton closeHelpButton;

	[SerializeField]
	private GameButton skillConfigButton;

	protected override void Init()
	{
		base.Init();
		InitUI(skillStoragePanel);
		helpButton.onClick.AddListener(_OnHelpButtonClicked);
		skillConfigButton.onClick.AddListener(_OnSkillConfigButtonClicked);
		closeHelpButton.onClick.AddListener(_OnCancelHelpButtonClicked);
		UIBase.fortressState.applicationState.eventManager.drawSkillEvent.AddListener(_OnDrawSkill);
		UIBase.fortressState.applicationState.eventManager.deleteSkillEvent.AddListener(_OnSkillRemove);
		UIBase.fortressState.applicationState.eventManager.addSkillWordEvent.AddListener(_OnSkillWordAdd);
		UIBase.fortressState.applicationState.eventManager.removeSkillWordEvent.AddListener(_OnSkillWordRemove);
		SetDirty();
	}

	protected override void Dispose()
	{
		UIBase.fortressState.applicationState.eventManager.drawSkillEvent.RemoveListener(_OnDrawSkill);
		UIBase.fortressState.applicationState.eventManager.deleteSkillEvent.RemoveListener(_OnSkillRemove);
		UIBase.fortressState.applicationState.eventManager.addSkillWordEvent.RemoveListener(_OnSkillWordAdd);
		UIBase.fortressState.applicationState.eventManager.removeSkillWordEvent.RemoveListener(_OnSkillWordRemove);
	}

	private void _OnDrawSkill(ProfileSkill skill)
	{
		SetDirty();
	}

	private void _OnSkillRemove(ProfileSkill skill)
	{
		SetDirty();
	}

	private void _OnSkillWordAdd((ProfileSkill, SkillWordDef) info)
	{
		SetDirty();
	}

	private void _OnSkillWordRemove((ProfileSkill, SkillWordDef) info)
	{
		SetDirty();
	}

	public void _OnSkillConfigButtonClicked()
	{
		UIBase.fortressState.uiManager.skillConfigPanel.Setup(UIBase.applicationState.newGameConfigManager.currentNewGameConfig.GetSkillConfigCharacterState(UIBase.applicationState));
		UIBase.fortressState.uiManager.skillConfigPanel.ShowUI();
		SetDirty();
	}

	private void _OnHelpButtonClicked()
	{
		helpHint.gameObject.SetActive(value: true);
	}

	private void _OnCancelHelpButtonClicked()
	{
		helpHint.gameObject.SetActive(value: false);
	}
}
