using UnityEngine;

public abstract class LevelWelcomeEvent : WorldEvent
{
	[SerializeField]
	protected WorldEventChoiceText prepareText;

	[SerializeField]
	protected WorldEventChoiceText activatePotentialTalentText;

	protected WorldEventChoice activatePotentialTalentChoice;

	public WorldEventChoice prepareSkillChoice;

	protected bool IsFirstStage => gameState.contractState.stage == 1;

	public override WorldEventPanel GetDisplayWorldEventPanel(GameStateInfo gameState)
	{
		return gameState.UIManager.worldEventPanel_Welcome;
	}

	protected override void Init()
	{
		base.Init();
		currentDisplayInfo.showLeaveButton = true;
		currentDisplayInfo.needRefresh = true;
		SetupActivatePotentialTalentChoice();
		if (IsFirstStage)
		{
			SetupPrepareSkillChoice();
		}
	}

	protected void SetupPrepareSkillChoice()
	{
		if (!gameState.playerState.PreparableSkills.IsEmptyOrNull())
		{
			prepareSkillChoice = new WorldEventChoice();
			prepareSkillChoice.descriptionText = prepareText.choiceDescription;
			prepareSkillChoice.buttonText = prepareText.choiceName;
			prepareSkillChoice.executeAction = PrepareSkill;
			prepareSkillChoice.stayAfterChoose = true;
			currentDisplayInfo.AddChoice(prepareSkillChoice);
		}
	}

	private void PrepareSkill()
	{
		EnqueueAction(new TriggerSelectSkillAction(_OnConfirmSkillSelect));
	}

	private void _OnConfirmSkillSelect(GameSkill skill)
	{
		currentDisplayInfo.RemoveChoice(prepareSkillChoice);
		currentDisplayInfo.description = prepareText.chosenDescription;
		SetPanelDirty();
	}

	protected void SetupActivatePotentialTalentChoice()
	{
		GameContractState contractState = gameState.contractState;
		if ((contractState == null || contractState.selectedContract?.contractDef?.contractRule?.CanActivatePotentialInLevelWelcome(gameState) != false) && !gameState.playerState.CurrentActivateAblePotentialTalent.IsEmptyOrNull())
		{
			activatePotentialTalentChoice = new WorldEventChoice();
			activatePotentialTalentChoice.descriptionText = activatePotentialTalentText.choiceDescription;
			activatePotentialTalentChoice.buttonText = activatePotentialTalentText.choiceName;
			activatePotentialTalentChoice.executeAction = ExecuteActivatePotentialTalentChoice;
			activatePotentialTalentChoice.stayAfterChoose = true;
			currentDisplayInfo.AddChoice(activatePotentialTalentChoice);
		}
	}

	private void ExecuteActivatePotentialTalentChoice()
	{
		ActivatePotentialTalentInfo t = new ActivatePotentialTalentInfo
		{
			onCancel = delegate
			{
				gameState.UIManager.activatePotentialTalentPanel.Unset();
			},
			onConfirm = delegate
			{
				currentDisplayInfo.RemoveChoice(activatePotentialTalentChoice);
				currentDisplayInfo.description = activatePotentialTalentText.chosenDescription;
				SetPanelDirty();
			}
		};
		gameState.UIManager.activatePotentialTalentPanel.Setup(t);
		gameState.UIManager.activatePotentialTalentPanel.SetActiveAndDirtyWithFadeAnim(active: true);
	}
}
