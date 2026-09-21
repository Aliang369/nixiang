using UnityEngine;

public class HealBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public int healCount;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.healBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_heal_health;

	public override string description => ScriptableObjectSingleton<TextDataPreset>.instance.term_heal_health_arg0.FormatWith(healCount);

	public override string countText => healCount.ToString();

	private HealBonus()
	{
	}

	public HealBonus(int healCount)
	{
		this.healCount = healCount;
	}

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_HealBonus;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		HealAction action = new HealAction(null, gameState.playerState.bft, healCount);
		gameState.actionManager.EnqueueAction(action);
		ConfirmGotBonus();
	}
}
