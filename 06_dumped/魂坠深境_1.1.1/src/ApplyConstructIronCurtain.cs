using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class ApplyConstructIronCurtain : Effect
{
	protected override NodeCastResult OnCast(GameEvent eve)
	{
		ItemContainer itemContainer = eve?.gameState?.playerState?.bft?.deckContainer;
		GameRandomManager gameRandomManager = eve?.gameState?.randomManager;
		if (itemContainer == null || gameRandomManager == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list = itemContainer.GetAllItems().Where(CharacterBlessingItemFilters.IsDeckArmor).ToList();
		if (list.Count == 0)
		{
			return NodeCastResult.Success;
		}
		foreach (Item item in list)
		{
			int num = CombatValueLimit.Clamp((int)item.counter);
			int num2 = CombatValueLimit.Clamp((long)num * 2L);
			if ((int)item.maxCounter != 0 && (int)item.maxCounter < num2)
			{
				Effect.EnqueueAction(eve, new SetMaxCounterAction(num2, item, eve.CasterInfluence));
			}
			Effect.EnqueueAction(eve, new ChangeCounterAction(num, item, eve.CasterInfluence, withPopupText: true));
		}
		Item targetItem = list.SelectRandom(gameRandomManager.NextDouble);
		Effect.EnqueueAction(eve, new RemoveItemAction(targetItem, eve.CasterInfluence));
		return NodeCastResult.Success;
	}
}
