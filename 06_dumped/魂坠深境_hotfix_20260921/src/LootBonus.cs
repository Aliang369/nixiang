using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class LootBonus : GetSingleItemBonus
{
	[SaveField(null, null)]
	public readonly Item loot;

	public override Sprite PreviewIcon => ScriptableObjectSingleton<DungeonGenerationManager>.instance.itemBonusIcon;

	public override string previewText => ScriptableObjectSingleton<TextDataPreset>.instance.term_loot_item;

	public LootBonus(GameStateInfo gameState)
	{
		if (gameState.currentEncounter?.enemyAI == null || gameState.currentEncounter.enemyAI.component.loots.IsNullOrEmpty())
		{
			loot = gameState.generationManager.GenerateRandomLoot(1).First();
		}
		else
		{
			loot = new Item(gameState.currentEncounter.enemyAI.component.loots.SelectRandom(gameState.randomManager.NextDouble));
		}
		item = loot;
	}

	private LootBonus()
	{
	}

	protected override void TriggerImpl(GameStateInfo gameState)
	{
		gameState.actionManager.EnqueueAction(new GatherMultiElementsAction(new MultiInfoDisplayPanel.SetupInfo
		{
			items = new List<Item> { item }
		}));
		ConfirmGotBonus();
	}
}
