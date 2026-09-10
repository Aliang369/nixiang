using UnityEngine;

public class AddItemSlotBonus : BattleBonusObject
{
	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.defaultBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_add_itemSlot_bonus;

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new ChangeSlotCountAction(gameState.playerState.TableContainer, 1, temp: false));
		ConfirmGotBonus();
	}
}
