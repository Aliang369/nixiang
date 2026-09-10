using System;
using System.Collections.Generic;

public class DreamEnergyDisturb : WorldEvent
{
	[LocalizeText("事件文本")]
	public WorldEventChoiceText acceptChoiceText;

	[LocalizeText("事件文本")]
	public WorldEventChoiceText refuseChoiceText;

	public ItemDefinition getItem;

	public int loseHealthPercent;

	public int GetCrystal;

	[NonSerialized]
	private WorldEventChoice acceptChoice;

	[NonSerialized]
	private WorldEventChoice leaveChoice;

	[NonSerialized]
	private WorldEventChoice refuseChoice;

	protected override void Init()
	{
		base.Init();
		acceptChoice = new WorldEventChoice();
		acceptChoice.buttonText = acceptChoiceText.choiceName;
		acceptChoice.AddDisplayItem(new Item(getItem));
		acceptChoice.descriptionText = string.Format(acceptChoiceText.choiceDescription, TextDataPreset.FormatItemName(gameState, acceptChoice.displayItems[0], out var _));
		acceptChoice.executeAction = delegate
		{
			EnqueueAction(new ChangeCrystalAction(-999));
			EnqueueGetMultiElementAction(new MultiInfoDisplayPanel.SetupInfo
			{
				items = new List<Item> { acceptChoice.displayItems[0] }
			});
			RefreshAfterChoose(acceptChoiceText.chosenDescription);
		};
		currentDisplayInfo.AddChoice(acceptChoice);
		int subHealth = (int)((float)(int)gameState.playerState.bft.maxHealth * ((float)loseHealthPercent / 100f));
		refuseChoice = new WorldEventChoice();
		refuseChoice.buttonText = refuseChoiceText.choiceName;
		refuseChoice.descriptionText = string.Format(refuseChoiceText.choiceDescription, subHealth, GetCrystal);
		refuseChoice.executeAction = delegate
		{
			EnqueueAction(new ChangeBftHealthAction(gameState.playerState.bft, -subHealth));
			EnqueueAction(new ChangeCrystalAction(GetCrystal));
			RefreshAfterChoose(refuseChoiceText.chosenDescription);
		};
		currentDisplayInfo.AddChoice(refuseChoice);
	}
}
