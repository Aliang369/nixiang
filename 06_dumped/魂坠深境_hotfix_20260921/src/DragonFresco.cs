using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragonFresco : WorldEvent
{
	[SerializeField]
	[LocalizeText("事件文本")]
	private string wizardExtraDescription;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string chooseSelectDescription;

	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	protected override void Init()
	{
		base.Init();
		if (gameState.playerState.CharacterClass == CharacterClassType.Wizard)
		{
			currentDisplayInfo.description = currentDisplayInfo.description + "\n" + wizardExtraDescription;
		}
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		List<Item> list = gameState.generationManager.GenerateCardRewardItems(3, gameState.difficultyState.CurrentStage, CardRewardType.Event);
		foreach (Item item2 in list)
		{
			EnqueueAction(new UpgradeItemAction(item2, null));
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectSingleItem(list, OnSelect, OnCancel, selected);
		selectItemInfo.canCancel = () => false;
		EnqueueAction(new NormalEventSelectItemAction(selectItemInfo));
		void OnCancel()
		{
			RefreshAfterChoose(choice1Text.chosenDescription);
		}
		void OnSelect()
		{
			Item item = selected.FirstOrDefault();
			if (item != null)
			{
				EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
				{
					items = new List<Item> { item.Copy() }
				});
			}
			RefreshAfterChoose(choice1Text.chosenDescription);
		}
	}

	private void ExecuteAction2()
	{
		List<Item> list = gameState.generationManager.GenerateRandomLoot(3);
		foreach (Item item2 in list)
		{
			EnqueueAction(new UpgradeItemAction(item2, null));
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectSingleItem(list, OnSelect, OnCancel, selected);
		selectItemInfo.canCancel = () => false;
		EnqueueAction(new NormalEventSelectItemAction(selectItemInfo));
		void OnCancel()
		{
			RefreshAfterChoose(choice2Text.chosenDescription);
		}
		void OnSelect()
		{
			Item item = selected.FirstOrDefault();
			if (item != null)
			{
				EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
				{
					items = new List<Item> { item.Copy() }
				});
			}
			RefreshAfterChoose(choice2Text.chosenDescription);
		}
	}
}
