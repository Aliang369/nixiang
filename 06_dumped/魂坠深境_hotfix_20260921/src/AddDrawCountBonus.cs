using UnityEngine;

public class AddDrawCountBonus : BattleBonusObject
{
	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.defaultBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_draw_item_count_bonus;

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		BattleFieldTransform bft = gameState.playerState.bft;
		bft.turnDrawItemCount = (int)bft.turnDrawItemCount + 1;
		ConfirmGotBonus();
	}
}
