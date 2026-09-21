using System.Collections.Generic;
using UnityEngine;

public class SelectIncreaseDefenceBonus : BattleBonusObject, PickBonusSelectionInfoPanel.IShowTextHint
{
	[SaveField(null, null)]
	public int strength;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.increaseDefenceBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_armor_enchant_arg0.FormatWith(strength);

	public override bool isChoose => true;

	string PickBonusSelectionInfoPanel.IShowTextHint.GetHint()
	{
		return ScriptableObjectSingleton<TextDataPreset>.instance.term_battleBonusObject_SelectIncreaseDefenceBonus;
	}

	private SelectIncreaseDefenceBonus()
	{
	}

	public SelectIncreaseDefenceBonus(int strength)
	{
		this.strength = strength;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		List<Item> selected = new List<Item>();
		List<Item> pool = new List<Item>(gameState.playerState.DeckContainer.GetAllItems());
		SelectItemInfo info = new SelectItemInfo
		{
			pool = pool,
			panelHintGetter = () => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_a_defence_card_add_counter_arg0.FormatWith(strength),
			canConfirm = () => selected.Count <= 1,
			canSelect = (Item it) => it.Defence,
			singleSelect = true,
			onConfirm = delegate
			{
				foreach (Item item in selected)
				{
					gameState.actionManager.EnqueueAction(new ChangeCounterAction(strength, item, null));
				}
				gameState.actionManager.EnqueueAction(new ShowMultiInfoAction(new MultiInfoDisplayPanel.SetupInfo
				{
					items = selected
				}));
				ConfirmGotBonus();
			},
			output = selected
		};
		gameState.actionManager.EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
	}
}
