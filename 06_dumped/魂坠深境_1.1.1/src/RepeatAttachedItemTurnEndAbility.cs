using System.Collections.Generic;

public class RepeatAttachedItemTurnEndAbility : Ability
{
	private const string ResolvingKey = "RepeatAttachedItemTurnEndAbility";

	public ValueGenerator extraTriggerCount;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, extraTriggerCount };
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.PlayerTurnEnd;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (base.CheckTriggerGameEventAble(eve))
		{
			Item item = eve.CasterRune?.attachItem;
			if (item != null && item.owner == eve.gameState?.playerState?.TableContainer && !item.IsAddAsSpellCost(eve.gameState))
			{
				return !eve.CasterRune.tempValues.ContainsKey("RepeatAttachedItemTurnEndAbility");
			}
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		Rune rune = eve.CasterRune;
		Item item = rune.attachItem;
		int valueOrDefault = (extraTriggerCount?.GenerateValue(eve)).GetValueOrDefault();
		for (int i = 0; i < valueOrDefault; i++)
		{
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				RepeatTurnEndEffect(eve.gameState, rune, item);
			}));
		}
	}

	private static void RepeatTurnEndEffect(GameStateInfo gameState, Rune rune, Item item)
	{
		if (rune?.attachItem != item || item?.owner != gameState?.playerState?.TableContainer || item.IsAddAsSpellCost(gameState))
		{
			return;
		}
		GameEvent gameEvent = new GameEvent(GameEvent.Type.PlayerTurnEnd, GameEvent.TriggerType.Real)
		{
			gameState = gameState
		};
		rune.tempValues["RepeatAttachedItemTurnEndAbility"] = true;
		try
		{
			gameEvent.SetCaster(item);
			if (item.CheckTriggerGameEventAble(gameEvent))
			{
				item.TriggerGameEvent(gameEvent);
			}
		}
		finally
		{
			gameEvent.SetCaster(null);
			rune.tempValues.Remove("RepeatAttachedItemTurnEndAbility");
		}
	}
}
