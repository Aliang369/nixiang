using System.Collections.Generic;
using UnityEngine;

public class SelectItemFromItemsBonus : SelectGetItemBonus
{
	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.itemBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_item;

	public override bool isChoose => true;

	private SelectItemFromItemsBonus()
	{
	}

	public SelectItemFromItemsBonus(List<Item> items)
	{
		allItems = items;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = selectItems
		}));
		ConfirmGotBonus();
	}
}
