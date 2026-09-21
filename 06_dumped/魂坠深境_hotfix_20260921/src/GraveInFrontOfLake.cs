using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraveInFrontOfLake : WorldEvent
{
	[SerializeField]
	[LocalizeText("事件文本")]
	private string wizardExtraDescription;

	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private WorldEventChoiceText choice3Text;

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
		if (gameState.playerState.CharacterClass == CharacterClassType.Wizard)
		{
			currentDisplayInfo.description = currentDisplayInfo.description + "\n" + wizardExtraDescription;
		}
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAbleChecker = () => gameState.playerState.HasEnoughCoin(8);
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
		if (gameState.playerState.CharacterClass != CharacterClassType.Wizard)
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
		EnqueueAction(new ChangeCoinAction(-8));
		Influence item = new Influence(gameState.generationManager.GenerateRandomCommonBless(1, updateRecord: true, QualityLevel.Common).FirstOrDefault());
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { item }
		});
		RefreshAfterChoose(choice1Text.chosenDescription);
	}

	private void ExecuteAction2()
	{
		EnqueueAction(new ChangeBftHealthAction(gameState.playerState.bft, 12));
		RefreshAfterChoose(choice2Text.chosenDescription);
	}

	private void ExecuteAction3()
	{
		EnqueueAction(new ChangeCoinAction(6));
		RefreshAfterChoose(choice3Text.chosenDescription);
	}
}
