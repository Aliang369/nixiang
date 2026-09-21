using System;
using System.Linq;
using UnityEngine;

public class TurnDamageValueGenerator : ValueGenerator
{
	private enum Type
	{
		First,
		Last,
		LastTurnSum,
		ThisTurnSum
	}

	[SerializeField]
	private Type type;

	[SerializeField]
	private bool dealByItem;

	[SerializeField]
	private bool dealToEnemy;

	[SerializeField]
	private bool dealToPlayer;

	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState?.recordManager == null)
		{
			return null;
		}
		Predicate<Damage> match = delegate(Damage d)
		{
			if (dealToEnemy)
			{
				BattleFieldTransform targetBFT = d.targetBFT;
				if (targetBFT == null || !targetBFT.isEnemy)
				{
					return false;
				}
			}
			if (dealByItem && !(d.source is Item))
			{
				return false;
			}
			if (dealToPlayer)
			{
				BattleFieldTransform targetBFT = d.targetBFT;
				if (targetBFT == null || !targetBFT.isPlayer)
				{
					return false;
				}
			}
			return true;
		};
		switch (type)
		{
		case Type.First:
		{
			Damage damage = eve.gameState.recordManager.damageThisRound.Find(match);
			if (damage == null)
			{
				return null;
			}
			return damage.subCounter;
		}
		case Type.Last:
		{
			Damage damage = eve.gameState.recordManager.damageThisRound.FindLast(match);
			if (damage == null)
			{
				return null;
			}
			return damage.subCounter;
		}
		case Type.LastTurnSum:
			if (eve.gameState.recordManager.damageLastRound.IsNullOrEmpty())
			{
				return null;
			}
			return eve.gameState.recordManager.damageLastRound.FindAll(match).Sum((Damage d) => d.subCounter);
		case Type.ThisTurnSum:
			if (eve.gameState.recordManager.damageThisRound.IsNullOrEmpty())
			{
				return null;
			}
			return eve.gameState.recordManager.damageThisRound.FindAll(match).Sum((Damage d) => d.subCounter);
		default:
			return null;
		}
	}
}
