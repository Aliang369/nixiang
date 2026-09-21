using System;
using System.Collections.Generic;
using UnityEngine;

public class ImprisonedHeretics : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private ItemDefinition itemDef;

	[SerializeField]
	private RuneDef runeDef;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	protected override void Init()
	{
		base.Init();
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription;
		choice1.buttonText = choice1Text.choiceName;
		choice1.AddDisplayItem(new Item(itemDef));
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.AddDisplayRune(new Rune(runeDef));
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { choice1.displayItems[0].Copy() }
		});
		RefreshAfterChoose(choice1Text.chosenDescription);
	}

	private void ExecuteAction2()
	{
		RefreshAfterChoose(choice2Text.chosenDescription);
		MintageRunePanel_Select.SetupInfo setupInfo = new MintageRunePanel_Select.SetupInfo(choice2.displayRunes);
		setupInfo.canBack = (Func<bool>)Delegate.Combine(setupInfo.canBack, (Func<bool>)(() => gameState.UIManager.mintageRunePanel_Select.GetRunes().Count == 0));
		gameState.UIManager.mintageRunePanel_Select.Setup(setupInfo);
	}
}
