using System.Collections;
using ChillyRoom.Services.ContainerOfSoul;
using Common.UI;
using UnityEngine;

public class CharacterInfoBasePanel : CellUI<PlayerCharacterState>, IStackPanel, IShowAsync
{
	[SerializeField]
	public CharacterInfoPanel characterInfoPanel;

	[SerializeField]
	private CharacterSelectorArea characterSelectorArea;

	[SerializeField]
	private GameButton backButton;

	[SerializeField]
	private GameButton helpButton;

	[SerializeField]
	public MasteryInfoPanel masteryInfoPanel;

	private bool triggerReturnToMainRoomOnClose;

	private bool postFirstGameTalentFocusMode;

	protected override void Init()
	{
		InitUI(characterInfoPanel);
		InitUI(characterSelectorArea);
		InitUI(masteryInfoPanel);
		backButton.onClick.AddListener(_OnBackButtonClicked);
		helpButton.onClick.AddListener(_OnHelpButtonClicked);
		characterSelectorArea.onSelect = delegate(PlayerCharacterState character)
		{
			UIHelper.HideAllInfo(null);
			characterInfoPanel.itemInfoPanel.HideAllCommonInfo();
			characterInfoPanel.skillInfoPanel.HideAllCommonInfo();
			masteryInfoPanel.Unset();
			characterInfoPanel.Setup(character);
		};
		void _OnBackButtonClicked()
		{
			if (base.setupData != null)
			{
				if (!base.setupData.locked && !base.setupData.isPreview && GlobalConfigManager.instance.enableNetwork)
				{
					GeneralGameAsyncOperationRequestOnly<UpdateCharacterSetupRequest> generalGameAsyncOperationRequestOnly = new GeneralGameAsyncOperationRequestOnly<UpdateCharacterSetupRequest>(new UpdateCharacterSetupRequest(base.setupData), CellUI<PlayerCharacterState>.applicationState.ChillyRoomSDK.Character.UpdateCharacterSetupAsync);
					generalGameAsyncOperationRequestOnly.OnComplete(delegate
					{
						manager.SetDirty();
					});
					generalGameAsyncOperationRequestOnly.EnqueueRun(ScriptableObjectSingleton<TextDataPreset>.instance.gameAsyncOperationText.term_loading_character_trial);
				}
				bool num = triggerReturnToMainRoomOnClose;
				triggerReturnToMainRoomOnClose = false;
				Unset();
				manager.HideUI(this);
				if (num)
				{
					CellUI<PlayerCharacterState>.applicationState.eventManager.returnToFortressMainRoomEvent.TriggerEvent();
				}
			}
		}
		static void _OnHelpButtonClicked()
		{
			CellUI<PlayerCharacterState>.fortressState.uiManager.fortressGuidePanel.characterInfoGuideHint.Show();
		}
	}

	IEnumerator IShowAsync.ShowAnim()
	{
		yield return CellUI<PlayerCharacterState>.applicationState.matterManager.SyncMatter();
		base.gameObject.SetActive(value: true);
	}

	public void SetupFromMainRoom(PlayerCharacterState setupData)
	{
		triggerReturnToMainRoomOnClose = setupData != null;
		postFirstGameTalentFocusMode = ShouldUsePostFirstGameTalentFocusMode(setupData);
		this.ShowUI(setupData);
	}

	protected override void OnSetup(PlayerCharacterState setupData)
	{
		base.gameObject.SetActive(value: true);
		characterInfoPanel.SetPostFirstGameTalentFocusMode(postFirstGameTalentFocusMode);
		characterInfoPanel.Setup(setupData);
		characterSelectorArea.Refresh(setupData, unlockedOnly: false, postFirstGameTalentFocusMode);
		SetDirty();
	}

	protected override void OnUnset()
	{
		postFirstGameTalentFocusMode = false;
		characterInfoPanel.SetPostFirstGameTalentFocusMode(active: false);
	}

	private bool ShouldUsePostFirstGameTalentFocusMode(PlayerCharacterState target)
	{
		if (target != null)
		{
			ApplicationStateInfo applicationStateInfo = CellUI<PlayerCharacterState>.applicationState;
			if (applicationStateInfo == null)
			{
				return false;
			}
			return applicationStateInfo.guideManager?.IsPostFirstGameTalentFocusMode == true;
		}
		return false;
	}
}
