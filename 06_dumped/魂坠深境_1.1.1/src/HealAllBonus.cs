using UnityEngine;

public class HealAllBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.healBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_heal_all_health;

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_HealAllBonus;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		HealAction action = new HealAction(null, gameState.playerState.bft, 999);
		gameState.actionManager.EnqueueAction(action);
		ConfirmGotBonus();
	}
}
