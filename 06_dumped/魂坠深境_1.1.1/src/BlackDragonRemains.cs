using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlackDragonRemains : WorldEvent
{
	[SerializeField]
	private WorldEventChoiceText choice1Text;

	[SerializeField]
	private WorldEventChoiceText choice2Text;

	[SerializeField]
	private WorldEventChoiceText choice3Text;

	public int choice1SubMaxHealth;

	public int choice2SubMaxHealth;

	public int choice3SubMaxHealth;

	[NonSerialized]
	private WorldEventChoice choice1;

	[NonSerialized]
	private WorldEventChoice choice2;

	[NonSerialized]
	private WorldEventChoice choice3;

	protected override void Init()
	{
		base.Init();
		choice1 = new WorldEventChoice();
		choice1.descriptionText = choice1Text.choiceDescription.FormatWith(choice1SubMaxHealth);
		choice1.buttonText = choice1Text.choiceName;
		choice1.executeAction = ExecuteAction1;
		choice1.executeAbleChecker = () => CanPayMaxHealthCost(choice1SubMaxHealth);
		choice1.stayAfterChoose = true;
		currentDisplayInfo.AddChoice(choice1);
		choice2 = new WorldEventChoice();
		choice2.descriptionText = choice2Text.choiceDescription.FormatWith(choice2SubMaxHealth);
		choice2.buttonText = choice2Text.choiceName;
		choice2.executeAction = ExecuteAction2;
		choice2.executeAbleChecker = () => CanPayMaxHealthCost(choice2SubMaxHealth);
		choice2.stayAfterChoose = true;
		currentDisplayInfo.AddChoice(choice2);
		choice3 = new WorldEventChoice();
		choice3.descriptionText = choice3Text.choiceDescription.FormatWith(GetPayableMaxHealthCost(choice3SubMaxHealth));
		choice3.buttonText = choice3Text.choiceName;
		choice3.executeAction = ExecuteAction3;
		currentDisplayInfo.AddChoice(choice3);
	}

	private void ExecuteAction1()
	{
		List<Item> pool = gameState.playerState.DeckContainer.GetAllItems();
		List<Item> output = new List<Item>();
		Action onConfirm = delegate
		{
			Item item = output[0];
			foreach (Rune rune in item.Runes)
			{
				foreach (Item item2 in pool)
				{
					if (item2.GetDef() == item.GetDef() && item2 != item)
					{
						EnqueueAction(new ChangeRuneAction(item2, rune, item));
					}
				}
			}
			EnqueueAction(new ChangeBftMaxHealthAction(gameState.playerState.bft, -choice1SubMaxHealth, baseAdd: true));
			RefreshAfterChoose(choice1Text.chosenDescription);
		};
		Func<Item, bool> selectableChecker = (Item item) => item.Runes.Any();
		Action onCancel = delegate
		{
		};
		Func<string> panelHintGetter = () => choice1.descriptionText;
		SelectItemInfo info = SelectItemInfo.SimpleSelectSingleItem(pool, onConfirm, onCancel, output, selectableChecker, panelHintGetter);
		EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
	}

	private void ExecuteAction2()
	{
		List<Item> allItems = gameState.playerState.DeckContainer.GetAllItems();
		List<Item> output = new List<Item>();
		SelectItemInfo info = SelectItemInfo.SimpleSelectSingleItem(onConfirm: delegate
		{
			Item item = output[0];
			foreach (Rune rune in item.Runes)
			{
				if (rune.def.stacking)
				{
					EnqueueAction(new ChangeRuneAction(item, rune, item));
				}
			}
			EnqueueAction(new ChangeBftMaxHealthAction(gameState.playerState.bft, -choice2SubMaxHealth, baseAdd: true));
			RefreshAfterChoose(choice1Text.chosenDescription);
		}, selectableChecker: (Item item) => item.Runes.Count((Rune r) => r.def.stacking) > 0, onCancel: delegate
		{
		}, panelHintGetter: () => choice2.descriptionText, pool: allItems, output: output);
		EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
	}

	private void ExecuteAction3()
	{
		int payableMaxHealthCost = GetPayableMaxHealthCost(choice3SubMaxHealth);
		EnqueueAction(new ChangeBftMaxHealthAction(gameState.playerState.bft, -payableMaxHealthCost, baseAdd: true));
		RefreshAfterChoose(choice3Text.chosenDescription);
	}

	private bool CanPayMaxHealthCost(int cost)
	{
		return (int)gameState.playerState.bft.maxHealth > cost;
	}

	private int GetPayableMaxHealthCost(int cost)
	{
		return Mathf.Min(cost, Mathf.Max((int)gameState.playerState.bft.maxHealth - 1, 0));
	}
}
