using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class RandomPoolSelectDefGenerator : ItemDefGenerator
{
	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private ItemDefGenerator poolGenerator;

	[SerializeField]
	private bool unique;

	public override List<ItemDefinition> GenerateItemDefs(GameEvent eve)
	{
		if (eve.gameState?.randomManager == null)
		{
			return null;
		}
		int count = 1;
		if (countGenerator != null)
		{
			int? num = countGenerator.GenerateValue(eve);
			if (!num.HasValue)
			{
				return null;
			}
			count = num.Value;
		}
		List<ItemDefinition> list = poolGenerator.GenerateItemDefs(eve);
		if (list == null)
		{
			return new List<ItemDefinition>();
		}
		if (!unique)
		{
			return list.SelectRandomCount(count, eve.gameState.randomManager.NextDouble);
		}
		return list.SelectRandomCountUnique(count, eve.gameState.randomManager.NextDouble);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return poolGenerator.GetNodeDisplayInfo(eve);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { poolGenerator, countGenerator };
	}
}
