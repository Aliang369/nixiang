using System;
using GameRandom;

public class CuriousMerchant : WorldEvent
{
	public WorldEventChoiceText sellChoiceText;

	public WorldEventChoiceText refuseChoiceText;

	public WorldEventChoiceText noIdeaChoiceText;

	[NonSerialized]
	private WorldEventChoice refuseChoice;

	[NonSerialized]
	private WorldEventChoice sellChoice;

	[NonSerialized]
	private Item target;

	protected override void Init()
	{
		base.Init();
		target = GetRandomPlayerItem();
		if (target == null)
		{
			WorldEventChoice worldEventChoice = new WorldEventChoice();
			worldEventChoice.buttonText = noIdeaChoiceText.choiceName;
			worldEventChoice.descriptionText = noIdeaChoiceText.choiceDescription;
			worldEventChoice.executeAction = delegate
			{
				RefreshAfterChoose(noIdeaChoiceText.chosenDescription);
			};
			currentDisplayInfo.AddChoice(worldEventChoice);
			return;
		}
		string arg = TextDataPreset.FormatItemName(gameState, target, out var _);
		currentDisplayInfo.description = EventDescription;
		int price = GetSellPrice(target.GetDef());
		sellChoice = new WorldEventChoice();
		sellChoice.descriptionText = string.Format(sellChoiceText.choiceDescription, arg, price);
		sellChoice.buttonText = string.Format(sellChoiceText.choiceName, arg, price);
		sellChoice.AddDisplayItem(target);
		sellChoice.executeAction = delegate
		{
			EnqueueAction(new ExileItemAction(target, null));
			EnqueueAction(new ChangeCoinAction(price));
			RefreshAfterChoose(sellChoiceText.chosenDescription);
		};
		currentDisplayInfo.AddChoice(sellChoice);
		WorldEventChoice worldEventChoice2 = new WorldEventChoice();
		worldEventChoice2.descriptionText = refuseChoiceText.choiceDescription;
		worldEventChoice2.buttonText = refuseChoiceText.choiceName;
		worldEventChoice2.executeAction = delegate
		{
			RefreshAfterChoose(refuseChoiceText.chosenDescription);
		};
		currentDisplayInfo.AddChoice(worldEventChoice2);
	}

	private Item GetRandomPlayerItem()
	{
		return gameState.playerState.DeckContainer.GetAllItems().SelectRandom(gameState.randomManager.NextDouble);
	}

	private int GetSellPrice(ItemDefinition item)
	{
		if (item.treasureValue != 0)
		{
			return item.treasureValue;
		}
		return item.qualityLevel switch
		{
			QualityLevel.Base => 5, 
			QualityLevel.Common => 8, 
			QualityLevel.Uncommon => 10, 
			QualityLevel.Rare => 12, 
			QualityLevel.Epic => 15, 
			_ => 0, 
		};
	}
}
