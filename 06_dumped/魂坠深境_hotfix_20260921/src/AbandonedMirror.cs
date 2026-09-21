using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbandonedMirror : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private ItemDefinition itemDefinition;

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
		choice1.executeAction = ExecuteAction1;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription;
		choice2.buttonText = choice2Text.choiceName;
		choice2.AddDisplayItem(new Item(itemDefinition));
		choice2.executeAction = ExecuteAction2;
		currentDisplayInfo.AddChoice(choice2);
	}

	private void ExecuteAction1()
	{
		currentDisplayInfo.ClearChoice();
		List<Item> allItems = gameState.playerState.DeckContainer.GetAllItems();
		List<Item> selected = new List<Item>();
		EnqueueAction(new NormalEventSelectItemAction(SelectItemInfo.SimpleSelectSingleItem(allItems, OnSelect, OnCancel, selected)));
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
		EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { choice2.displayItems[0].Copy() }
		});
		RefreshAfterChoose(choice2Text.chosenDescription);
	}
}
