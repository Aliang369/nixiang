using System.Collections.Generic;

public class TurnTriggerAbility : Ability
{
	public enum OwnerTriggerType
	{
		Self,
		Opponent,
		Player,
		Enemy
	}

	public enum TurnType
	{
		turnStart = 0,
		turnEnd = 1,
		TurnStartAfterDrawItems = 100
	}

	public Effect effect;

	public OwnerTriggerType ownerType;

	public bool requireAfterTurnEnd;

	public TurnType turnType;

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { triggerCheckNode, effect };
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (CheckTriggerGameEventAble(eve))
		{
			TriggerEffect(eve, effect);
		}
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (!base.CheckTriggerGameEventAble(eve))
		{
			return false;
		}
		bool flag = (eve.CasterItem != null && eve.CasterItem.afterTurnEnd) || (eve.CasterEnchant != null && eve.CasterEnchant.afterTurnEnd) || (eve.CasterItem == null && eve.CasterEnchant == null);
		if (requireAfterTurnEnd && !flag)
		{
			return false;
		}
		if (CheckTurnType(eve) && CheckOwnerType(eve))
		{
			return true;
		}
		return false;
	}

	private bool CheckTurnType(GameEvent eve)
	{
		if ((turnType != TurnType.turnStart || (eve.type != GameEvent.Type.PlayerTurnStart && eve.type != GameEvent.Type.EnemyTurnStart)) && (turnType != TurnType.turnEnd || (eve.type != GameEvent.Type.PlayerTurnEnd && eve.type != GameEvent.Type.EnemyTurnEnd)))
		{
			if (turnType == TurnType.TurnStartAfterDrawItems)
			{
				return eve.type == GameEvent.Type.AfterTurnStartDrawItems;
			}
			return false;
		}
		return true;
	}

	private bool CheckOwnerType(GameEvent eve)
	{
		switch (ownerType)
		{
		case OwnerTriggerType.Self:
			if (eve.CasterOwner != null && eve.CasterOwner.isEnemy)
			{
				return CheckEnemyTurn();
			}
			if (eve.CasterOwner != null && eve.CasterOwner.isPlayer)
			{
				return CheckPlayerTurn();
			}
			break;
		case OwnerTriggerType.Opponent:
			if (eve.CasterOwner != null && eve.OpponentBFT.isEnemy)
			{
				return CheckEnemyTurn();
			}
			if (eve.CasterOwner != null && eve.OpponentBFT.isPlayer)
			{
				return CheckPlayerTurn();
			}
			break;
		case OwnerTriggerType.Player:
			return CheckPlayerTurn();
		case OwnerTriggerType.Enemy:
			return CheckEnemyTurn();
		}
		return false;
		bool CheckEnemyTurn()
		{
			if (eve.type != GameEvent.Type.EnemyTurnEnd)
			{
				return eve.type == GameEvent.Type.EnemyTurnStart;
			}
			return true;
		}
		bool CheckPlayerTurn()
		{
			if (eve.type != GameEvent.Type.PlayerTurnEnd && eve.type != GameEvent.Type.PlayerTurnStart)
			{
				return eve.type == GameEvent.Type.AfterTurnStartDrawItems;
			}
			return true;
		}
	}

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.PlayerTurnStart;
		yield return GameEvent.Type.PlayerTurnEnd;
		yield return GameEvent.Type.EnemyTurnStart;
		yield return GameEvent.Type.EnemyTurnEnd;
		yield return GameEvent.Type.AfterTurnStartDrawItems;
	}
}
