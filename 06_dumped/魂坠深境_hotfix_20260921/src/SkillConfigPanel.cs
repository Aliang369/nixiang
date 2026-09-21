using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using UnityEngine;

public class SkillConfigPanel : UIBase, IStackPanel, IShowAsync, IHideAsync
{
	[SerializeField]
	private CharacterSelectorArea characterSelectorArea;

	[SerializeField]
	public SkillConfigPresetPanel skillConfigPresetPanel;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton showHintButton;

	[SerializeField]
	private Animator animator;

	private PlayerCharacterState character => characterSelectorArea.selectedCharacter;

	protected override void Init()
	{
		InitUI(characterSelectorArea);
		InitUI(skillConfigPresetPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		showHintButton.onClick.AddListener(_OnShowHintButtonClicked);
		characterSelectorArea.onSelect = SelectCharacter;
	}

	public void Setup(PlayerCharacterState selectedCharacter)
	{
		characterSelectorArea.Refresh(selectedCharacter, unlockedOnly: true);
		SelectCharacter(characterSelectorArea.selectedCharacter ?? selectedCharacter);
		SetDirty();
	}

	public void SelectCharacter(PlayerCharacterState character)
	{
		if (character != null)
		{
			SaveSkillPreset();
			skillConfigPresetPanel.Setup(character);
			SetDirty();
		}
	}

	private void _OnShowHintButtonClicked()
	{
		UIBase.fortressState.uiManager.fortressGuidePanel.skillConfigGuideHint.Show();
	}

	private void _OnBackButtonClicked()
	{
		SaveSkillPreset();
		manager.HideUI(this);
		UIBase.fortressState.uiManager.SetDirty();
	}

	IEnumerator IShowAsync.ShowUIAsync()
	{
		base.gameObject.SetActive(value: true);
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.show);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
	}

	IEnumerator IHideAsync.HideUIAsync()
	{
		((Behaviour)(object)animator).enabled = true;
		animator.SetTrigger(AnimKeywords.hide);
		yield return new WaitUntil(() => !((Behaviour)(object)animator).enabled);
		base.gameObject.SetActive(value: false);
	}

	private void SaveSkillPreset()
	{
		if (GlobalConfigManager.instance.enableNetwork)
		{
			new GeneralGameAsyncOperationRequestOnly<UpdateCharacterSetupRequest>(new UpdateCharacterSetupRequest(character), UIBase.applicationState.ChillyRoomSDK.Character.UpdateCharacterSetupAsync).EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_saving_character_setup);
		}
		UIBase.applicationState.savingManager.SetProfileDirtyToSave();
	}
}
