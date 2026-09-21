using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class RandomInfluenceDefGenerator : InfluenceDefGenerator
{
	[SerializeField]
	private int count;

	[SerializeField]
	private InfluenceDefGenerator pool;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		List<InfluenceDef> source = pool.GenerateInfluenceDefs(eve);
		if (source.IsNullOrEmpty())
		{
			return null;
		}
		return source.SelectRandomCount(count, eve.gameState.randomManager.NextDouble);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (pool != null)
		{
			list.Add(pool);
		}
		return list;
	}
}
