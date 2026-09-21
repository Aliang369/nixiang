using System;

public class RuinsWorldEvent : WorldEvent
{
	public WorldEventChoiceText repairChoiceText;

	public WorldEventChoiceText resonateChoiceText;

	public WorldEventChoiceText lootChoiceText;

	public ItemDefinition repairRewardItem;

	public int repairWillPowerCost = 1;

	public int resonateWillPowerCost = 2;

	public int lootCoin = 10;

	[NonSerialized]
	private WorldEventChoice repairChoice;

	[NonSerialized]
	private WorldEventChoice resonateChoice;

	[NonSerialized]
	private WorldEventChoice lootChoice;

	protected override void Init()
	{
		base.Init();
		Item item = ((repairRewardItem != null) ? new Item(repairRewardItem) : null);
		string text = ((item != null) ? TextDataPreset.FormatItemName(gameState, item, out var _) : string.Empty);
		repairChoice = new WorldEventChoice
		{
			buttonText = repairChoiceText.choiceName,
			descriptionText = repairChoiceText.choiceDescription.FormatWith(TextDisplayHelper.NumToStringWithSign(repairWillPowerCost), text),
			executeAbleChecker = () => repairRewardItem != null,
			executeAction = ExecuteRepairChoice,
			stayAfterChoose = true
		};
		if (item != null)
		{
			repairChoice.AddDisplayItem(item);
		}
		currentDisplayInfo.AddChoice(repairChoice);
		resonateChoice = new WorldEventChoice
		{
			buttonText = resonateChoiceText.choiceName,
			descriptionText = resonateChoiceText.choiceDescription.FormatWith(TextDisplayHelper.NumToStringWithSign(resonateWillPowerCost)),
			executeAbleChecker = HasActivatablePotential,
			executeAction = ExecuteResonateChoice,
			stayAfterChoose = true
		};
		currentDisplayInfo.AddChoice(resonateChoice);
		lootChoice = new WorldEventChoice
		{
			buttonText = lootChoiceText.choiceName,
			descriptionText = lootChoiceText.choiceDescription.FormatWith(lootCoin.ToString().ColorText(ScriptableObjectSingleton<VFXDataPreset>.instance.coinColor)),
			executeAction = ExecuteLootChoice,
			stayAfterChoose = true
		};
		currentDisplayInfo.AddChoice(lootChoice);
	}

	private bool HasActivatablePotential()
	{
		return !gameState.playerState.CurrentActivateAblePotentialTalent.IsEmptyOrNull();
	}

	private void ExecuteRepairChoice()
	{
		if (!(repairRewardItem == null))
		{
			ConfirmWillPowerCost(repairWillPowerCost, delegate
			{
				EnqueueAction(new ChangeAdditionalWillPowerCostAction(repairWillPowerCost));
				EnqueueAction(new SpawnItemAction(new Item(repairRewardItem), gameState.playerState.DeckContainer));
				RefreshAfterChoose(repairChoiceText.chosenDescription);
			});
		}
	}

	private void ExecuteResonateChoice()
	{
		if (HasActivatablePotential())
		{
			ConfirmWillPowerCost(resonateWillPowerCost, OpenPotentialSelection);
		}
	}

	private void OpenPotentialSelection()
	{
		if (HasActivatablePotential())
		{
			ActivatePotentialTalentInfo t = new ActivatePotentialTalentInfo
			{
				onCancel = delegate
				{
					gameState.UIManager.activatePotentialTalentPanel.Unset();
				},
				onConfirm = delegate
				{
					EnqueueAction(new ChangeAdditionalWillPowerCostAction(resonateWillPowerCost));
					RefreshAfterChoose(resonateChoiceText.chosenDescription);
				}
			};
			gameState.UIManager.activatePotentialTalentPanel.Setup(t);
			gameState.UIManager.activatePotentialTalentPanel.SetActiveAndDirtyWithFadeAnim(active: true);
		}
	}

	private void ExecuteLootChoice()
	{
		EnqueueAction(new ChangeCoinAction(lootCoin));
		RefreshAfterChoose(lootChoiceText.chosenDescription);
	}

	private void ConfirmWillPowerCost(int cost, Action onConfirm)
	{
		if (!gameState.playerState.ShouldShowWillPowerOverLimitConfirm(cost, out var overLimitWillPower, out var penaltyInfluences))
		{
			onConfirm?.Invoke();
			return;
		}
		gameState.UIManager.commonSecondConfirmPanel.SetupWillPowerOverLimit(new WillPowerOverLimitConfirmInfo
		{
			overLimitWillPower = overLimitWillPower,
			penaltyInfluences = penaltyInfluences,
			confirmCallback = onConfirm
		});
	}
}
