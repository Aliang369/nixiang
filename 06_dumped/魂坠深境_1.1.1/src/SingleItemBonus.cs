using System.Collections.Generic;
using UnityEngine;

public class SingleItemBonus : GetSingleItemBonus
{
	public enum GetItemBonusType
	{
		None,
		BonusGame,
		Event
	}

	[SaveField(null, null)]
	public GetItemBonusType type;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.itemBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_get_item;

	private SingleItemBonus()
	{
	}

	public SingleItemBonus(Item selectItem, GetItemBonusType type = GetItemBonusType.None)
	{
		item = selectItem;
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { item }
		}));
		if (type == GetItemBonusType.BonusGame)
		{
			ThinkingAnalyticsExtends.TrackExposeBonusCard(gameState, item, "游艺", selectable: true);
		}
		ConfirmGotBonus();
	}
}
