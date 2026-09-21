using System;
using System.Collections.Generic;
using UnityEngine;

public class UnfinishedGolem : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string failedText;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private ItemDefinition golemDed;

	[SerializeField]
	private float baseChance = 0.3f;

	[SerializeField]
	private float deltaChance = 0.1f;

	[SerializeField]
	private int lostHealth = 8;

	[SerializeField]
	[LocalizeText("事件文本")]
	private string failedActivateGolemDescription;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	[NonSerialized]
	private int tryCount;

	private float CurrentChance => baseChance + (float)tryCount * deltaChance;

	protected override void Init()
	{
		base.Init();
		tryCount = 0;
		choice1 = new WorldEventChoice();
		choice1.descriptionText = string.Format(choice1Text.choiceDescription, lostHealth, (int)(CurrentChance * 100f));
		choice1.buttonText = choice1Text.choiceName;
		choice1.AddDisplayItem(new Item(golemDed));
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
		EnqueueAction(new ChangeBftHealthAction(gameState.playerState.bft, -lostHealth));
		if (gameState.randomManager.NextDouble(0.0, 1.0) < (double)CurrentChance)
		{
			EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
			{
				items = new List<Item> { choice1.displayItems[0].Copy() }
			});
			RefreshAfterChoose(choice1Text.chosenDescription);
		}
		else
		{
			tryCount++;
			(gameState.UIManager.currentEventPanel as NormalEventPanel).SetDescription(failedText);
			choice1.descriptionText = string.Format(choice1Text.choiceDescription, lostHealth, (int)(CurrentChance * 100f));
			RefreshAfterChoose(failedActivateGolemDescription, new List<WorldEventChoice>(currentDisplayInfo.choices), showLeaveButton: false);
			SetPanelDirty();
		}
	}

	private void ExecuteAction2()
	{
		EnqueueAction(new ChangeCrystalAction(4));
		RefreshAfterChoose(choice2Text.chosenDescription);
	}
}
