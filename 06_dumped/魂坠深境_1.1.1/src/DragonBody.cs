using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DragonBody : WorldEvent
{
	[SerializeField]
	private string wizardExtraDescription;

	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private InfluenceDef def;

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
		choice1.executeAction = Stare;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.AddDisplayInfluence(new Influence(def));
		choice2.executeAction = Pray;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void Stare()
	{
		currentDisplayInfo.ClearChoice();
		List<Item> allItems = gameState.playerState.DeckContainer.GetAllItems();
		List<Item> selected = new List<Item>();
		EnqueueAction(new NormalEventSelectItemAction(SelectItemInfo.SimpleSelectSingleItem(allItems, OnSelect, OnCancel, selected, null, () => choice1Text.choiceDescription)));
		void OnCancel()
		{
			RefreshAfterChoose(choice1Text.chosenDescription);
		}
		void OnSelect()
		{
			EnqueueAction(new RemoveItemAction(selected.FirstOrDefault(), null));
			RefreshAfterChoose(choice1Text.chosenDescription);
		}
	}

	private void Pray()
	{
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			influences = new List<Influence> { choice2.displayInfluences[0] }
		});
		RefreshAfterChoose(choice2Text.chosenDescription);
	}
}
