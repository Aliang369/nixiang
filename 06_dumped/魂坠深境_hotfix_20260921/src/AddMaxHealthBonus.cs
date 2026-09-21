using UnityEngine;

public class AddMaxHealthBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public int healthCount;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.addMaxHealthBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_add_max_health;

	public override string countText => healthCount.ToString();

	private AddMaxHealthBonus()
	{
	}

	public AddMaxHealthBonus(int healthCount)
	{
		this.healthCount = healthCount;
	}

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_AddMaxHealthBonus;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new ChangeBftMaxHealthAction(gameState.playerState.bft, healthCount, baseAdd: true));
		ConfirmGotBonus();
	}
}
