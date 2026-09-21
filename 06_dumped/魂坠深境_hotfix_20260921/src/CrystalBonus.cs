using UnityEngine;

public class CrystalBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public int count;

	public override string description => ScriptableObjectSingleton<TextDataPreset>.instance.term_get_args_crystal.FormatWith(count);

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.crystalBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_get_crystal;

	public override string countText => $"{count}";

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_CrystalBonus;
	}

	public CrystalBonus(int count)
	{
		this.count = count;
	}

	private CrystalBonus()
	{
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			crystal = count
		}));
		ConfirmGotBonus();
	}
}
