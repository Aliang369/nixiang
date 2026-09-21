using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckDealDamageInTurn : CheckNode
{
	private enum TurnType
	{
		ThisTurn,
		LastTurn
	}

	[SerializeField]
	private DamageCheck damageCheck;

	[SerializeField]
	private TurnType turnType;

	public override bool? Check(GameEvent eve)
	{
		return ((IEnumerable<Damage>)(turnType switch
		{
			TurnType.ThisTurn => eve.gameState?.recordManager?.damageThisRound, 
			TurnType.LastTurn => eve.gameState?.recordManager?.damageLastRound, 
			_ => throw new ArgumentOutOfRangeException(), 
		}))?.Any(delegate(Damage da)
		{
			DamageCheck obj = damageCheck;
			return obj != null && obj.Check(eve, da) == true;
		});
	}
}
