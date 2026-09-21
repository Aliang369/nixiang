using System.Collections.Generic;

public class InfiniteAbility : Ability
{
	public InfiniteAbility()
	{
		occasion = TriggerOccasion.InTableItem;
		abilityAnimType = AbilityAnimType.Positive;
		animPriority = AbilityAnimPriority.Strong;
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.EnemyTurnStart;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		Item casterItem = eve.CasterItem;
		BattleFieldTransform battleFieldTransform = casterItem?.OwnerBft;
		if (battleFieldTransform == null || !casterItem.IsPrimaryInfiniteSource())
		{
			return false;
		}
		if (battleFieldTransform.isPlayer ? (eve.type == GameEvent.Type.PlayerTurnStart) : (eve.type == GameEvent.Type.EnemyTurnStart))
		{
			return battleFieldTransform.tableContainer.ItemsCount < battleFieldTransform.tableContainer.GetFinalFixedSlotCount();
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		Item item = eve.CasterItem;
		if (IsPlayerAnimAble(eve))
		{
			TriggerAnim(eve.gameState, item);
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				eve.gameState.gameEventManager.TriggerAfterTriggerAbility(item);
			}));
		}
		eve.gameState.actionManager.EnqueueAction(new RefillEnemyIntentAction(item.OwnerBft, item));
		if (IsPlayerAnimAble(eve))
		{
			eve.gameState.actionManager.EnqueueAction(new ActionAction(delegate
			{
				TriggerAnimEnd(eve.gameState, item);
			}));
		}
	}
}
