using System.Collections.Generic;
using System.Linq;

public class OpeningPerformanceAbility : Ability
{
	private const string TriggeredThisTurnKey = "OpeningPerformance.TriggeredThisTurn";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.DrawFromPile;
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.BattleStart;
		yield return GameEvent.Type.BeforeBattleEnd;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		Influence influence = eve?.CasterInfluence;
		if (influence == null)
		{
			return;
		}
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.PlayerTurnStart || type == GameEvent.Type.BattleStart || type == GameEvent.Type.BeforeBattleEnd)
		{
			SetTriggeredThisTurn(influence, value: false);
		}
		else
		{
			if (!TryGetDrawnItem(eve, influence, out var item) || HasTriggeredThisTurn(influence))
			{
				return;
			}
			bool flag = IsAdvanceableMusicChapter(item);
			if (!flag && !CanTriggerItemTurnEnd(item, eve.gameState))
			{
				return;
			}
			SetTriggeredThisTurn(influence, value: true);
			TriggerAnim(eve.gameState, influence);
			if (flag)
			{
				EnqueueMusicChapter(eve.gameState, item, influence);
				return;
			}
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				TriggerItemTurnEnd(item, influence);
			}));
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		Influence influence = eve?.CasterInfluence;
		if (influence?.OwnerBft == null)
		{
			return false;
		}
		GameEvent.Type type = eve.type;
		if (type == GameEvent.Type.PlayerTurnStart || type == GameEvent.Type.BattleStart || type == GameEvent.Type.BeforeBattleEnd)
		{
			return true;
		}
		if (eve.type == GameEvent.Type.DrawFromPile && !HasTriggeredThisTurn(influence) && TryGetDrawnItem(eve, influence, out var item))
		{
			if (!IsAdvanceableMusicChapter(item))
			{
				return CanTriggerItemTurnEnd(item, eve.gameState);
			}
			return true;
		}
		return false;
	}

	private static bool TryGetDrawnItem(GameEvent eve, Influence influence, out Item item)
	{
		item = eve?.TargetItem;
		if (eve != null && eve.type == GameEvent.Type.DrawFromPile && item != null && item.OwnerBft == influence.OwnerBft && item.owner == influence.OwnerBft.tableContainer)
		{
			return !item.IsAddAsSpellCost(eve.gameState);
		}
		return false;
	}

	private static bool IsAdvanceableMusicChapter(Item item)
	{
		if (item != null && item.def?.tags?.Any((ItemTag tag) => (object)tag != null && tag.type == ItemTag.Type.Music) == true && (int)item.maxCounter > 0)
		{
			return (int)item.counter < (int)item.maxCounter;
		}
		return false;
	}

	private static bool CanTriggerItemTurnEnd(Item item, GameStateInfo gameState)
	{
		if (item == null || gameState == null)
		{
			return false;
		}
		GameEvent gameEvent = CreateItemTurnEndEvent(item, gameState);
		try
		{
			return item.CheckTriggerGameEventAble(gameEvent);
		}
		finally
		{
			gameEvent.SetCaster(null);
		}
	}

	private static void EnqueueMusicChapter(GameStateInfo gameState, Item item, Influence influence)
	{
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerAddChapterResolve(item, influence, GameEvent.TriggerType.Real);
		if (!resolveInfo.reject)
		{
			gameState.actionManager.EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_slot_addChapter, item));
			gameState.actionManager.EnqueueAction(new ChangeCounterAction(resolveInfo.targetDelta + 1, item, influence, withPopupText: true));
		}
	}

	private static void TriggerItemTurnEnd(Item item, Influence influence)
	{
		GameStateInfo gameStateInfo = influence?.OwnerBft?.gameState;
		if (gameStateInfo == null || influence?.OwnerBft == null || item.OwnerBft != influence.OwnerBft || item.owner != influence.OwnerBft.tableContainer)
		{
			return;
		}
		GameEvent gameEvent = CreateItemTurnEndEvent(item, gameStateInfo);
		try
		{
			item.TriggerGameEvent(gameEvent);
		}
		finally
		{
			gameEvent.SetCaster(null);
		}
	}

	private static GameEvent CreateItemTurnEndEvent(Item item, GameStateInfo gameState)
	{
		GameEvent gameEvent = new GameEvent(GameEvent.Type.PlayerTurnEnd, GameEvent.TriggerType.Real);
		gameEvent.gameState = gameState;
		gameEvent.SetCaster(item);
		return gameEvent;
	}

	private static bool HasTriggeredThisTurn(Influence influence)
	{
		if (influence.tempValues != null && influence.tempValues.TryGetValue("OpeningPerformance.TriggeredThisTurn", out var value))
		{
			if (value is bool)
			{
				return (bool)value;
			}
			return false;
		}
		return false;
	}

	private static void SetTriggeredThisTurn(Influence influence, bool value)
	{
		if (influence.tempValues == null)
		{
			influence.tempValues = new Dictionary<string, object>();
		}
		influence.tempValues["OpeningPerformance.TriggeredThisTurn"] = value;
	}
}
