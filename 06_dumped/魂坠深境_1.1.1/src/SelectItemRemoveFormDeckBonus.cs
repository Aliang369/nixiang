using System.Collections.Generic;
using UnityEngine;

public class SelectItemRemoveFormDeckBonus : BattleBonusObject
{
	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.defaultBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_remove_one_item;

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		List<Item> pool = new List<Item>(gameState.playerState.DeckContainer.GetAllItems());
		List<Item> selected = new List<Item>();
		SelectItemInfo info = new SelectItemInfo
		{
			pool = pool,
			panelHintGetter = () => previewText,
			canConfirm = () => selected.Count <= 1,
			canSelect = (Item _) => true,
			singleSelect = true,
			onConfirm = delegate
			{
				foreach (Item item in selected)
				{
					gameState.actionManager.EnqueueAction(new RemoveItemAction(item, null));
				}
				ConfirmGotBonus();
			},
			output = selected
		};
		gameState.actionManager.EnqueueAction(new NormalEventSelectItemAction(info));
	}
}
