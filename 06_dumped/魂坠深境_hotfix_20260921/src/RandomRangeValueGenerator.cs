using System.Collections.Generic;
using UnityEngine;

public class RandomRangeValueGenerator : ValueGenerator
{
	[SerializeField]
	private ValueGenerator upperBoundGenerator;

	[SerializeField]
	private ValueGenerator lowerBoundGenerator;

	public override int? GenerateValue(GameEvent eve)
	{
		GameRandomManager gameRandomManager = eve.gameState?.randomManager;
		if (gameRandomManager == null)
		{
			return null;
		}
		int? num = lowerBoundGenerator.GenerateValue(eve);
		int? num2 = upperBoundGenerator.GenerateValue(eve);
		if (!num.HasValue || !num2.HasValue)
		{
			return null;
		}
		if (num > num2)
		{
			int? num3 = num2;
			int? num4 = num;
			num = num3;
			num2 = num4;
		}
		return gameRandomManager.Next(num.Value, num2.Value);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return null;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { upperBoundGenerator, lowerBoundGenerator };
	}
}
