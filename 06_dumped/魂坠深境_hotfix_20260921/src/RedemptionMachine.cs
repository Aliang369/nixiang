using System;
using System.Collections.Generic;
using UnityEngine;

public class RedemptionMachine : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string failedText;

	[SerializeField]
	private WorldEventChoiceText choice3Text;

	[SerializeField]
	private ItemDefinition itemDefinition;

	[SerializeField]
	private int minGetCoin = 1;

	[SerializeField]
	private int maxGetCoin = 4;

	[SerializeField]
	private int unlockGetCoin = 4;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice choice3;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	protected override void Init()
	{
		base.Init();
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.AddDisplayItem(new Item(itemDefinition));
		choice1.executeAbleChecker = () => gameState.playerState.HasEnoughCoin(5);
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
		if (gameState.playerState.CharacterClass == CharacterClassType.Warrior)
		{
			choice3 = new WorldEventChoice();
			choice3.descriptionText = choice3Text.choiceDescription;
			choice3.buttonText = choice3Text.choiceName;
			choice3.executeAction = ExecuteAction3;
			currentDisplayInfo.AddChoice(choice3);
		}
	}

	private void ExecuteAction1()
	{
		EnqueueAction(new ChangeCoinAction(-5));
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { choice1.displayItems[0].Copy() }
		});
		RefreshAfterChoose(choice1Text.chosenDescription);
	}

	private void ExecuteAction2()
	{
		int delta = minGetCoin + Mathf.RoundToInt(gameState.randomManager.NextFloat() * (float)(maxGetCoin - minGetCoin));
		EnqueueAction(new ChangeCoinAction(delta));
		RefreshAfterChoose(choice2Text.chosenDescription);
	}

	private void ExecuteAction3()
	{
		EnqueueAction(new ChangeCoinAction(unlockGetCoin));
		RefreshAfterChoose(choice3Text.chosenDescription);
	}
}
