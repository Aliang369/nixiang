using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class SelectRandomInstantiateGenerator : NewItemGenerator
{
	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private NewItemGenerator pool;

	[SerializeField]
	private bool tryUnique;

	public override List<Item> GenerateNewItems(GameEvent eve)
	{
		if (eve.gameState?.randomManager == null)
		{
			return null;
		}
		List<Item> list = pool.GenerateNewItems(eve);
		if (list == null)
		{
			return null;
		}
		if (list.Count == 0 || eve.IsDisplayEvent())
		{
			return new List<Item>();
		}
		int? num = ((countGenerator != null) ? countGenerator.GenerateValue(eve) : new int?(1));
		if (!num.HasValue)
		{
			return null;
		}
		if (!tryUnique)
		{
			return list.SelectRandomCount(num.Value, eve.gameState.randomManager.NextDouble);
		}
		return list.SelectRandomCountUnique(num.Value, eve.gameState.randomManager.NextDouble);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { pool, countGenerator };
	}
}
