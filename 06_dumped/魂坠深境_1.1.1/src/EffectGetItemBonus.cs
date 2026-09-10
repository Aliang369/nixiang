using System;
using UnityEngine;

public class EffectGetItemBonus : SelectGetItemBonus
{
	public Action<Item> selectConfirmAction;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.itemBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_item;

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		selectConfirmAction?.Invoke(selectItems[0]);
		ConfirmGotBonus();
	}
}
