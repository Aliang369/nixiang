using UnityEngine;

public class CoinBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public int count;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.coinBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_get_coin;

	public override string description => ScriptableObjectSingleton<TextDataPreset>.instance.term_get_args_coin.FormatWith(count);

	public override string countText => $"{count}";

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_CoinBonus;
	}

	public CoinBonus(int count)
	{
		this.count = count;
	}

	private CoinBonus()
	{
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			coin = count
		}));
		ConfirmGotBonus();
	}
}
