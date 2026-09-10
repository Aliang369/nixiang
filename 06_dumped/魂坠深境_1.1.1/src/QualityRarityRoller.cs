using System;

public static class QualityRarityRoller
{
	public static QualityLevel Roll(DungeonGenerationManager.CardRewardRarityWeights weights, bool hasCommon, bool hasUncommon, bool hasRare, double randomValue, out double withinRarityValue)
	{
		return Roll(weights.common, weights.uncommon, weights.rare, hasCommon, hasUncommon, hasRare, randomValue, out withinRarityValue);
	}

	public static QualityLevel Roll(int common, int uncommon, int rare, bool hasCommon, bool hasUncommon, bool hasRare, double randomValue, out double withinRarityValue)
	{
		double num = (hasCommon ? Math.Max(0.0, common) : 0.0);
		double num2 = (hasUncommon ? Math.Max(0.0, uncommon) : 0.0);
		double num3 = (hasRare ? Math.Max(0.0, rare) : 0.0);
		double num4 = num + num2 + num3;
		if (num4 <= 0.0)
		{
			throw new InvalidOperationException("没有可用的稀有度权重池。");
		}
		double num5 = Math.Min(Math.Max(randomValue, 0.0), 0.9999999999999999) * num4;
		if (num5 < num)
		{
			withinRarityValue = ((num > 0.0) ? (num5 / num) : 0.0);
			return QualityLevel.Common;
		}
		num5 -= num;
		if (num5 < num2)
		{
			withinRarityValue = ((num2 > 0.0) ? (num5 / num2) : 0.0);
			return QualityLevel.Uncommon;
		}
		num5 -= num2;
		withinRarityValue = ((num3 > 0.0) ? (num5 / num3) : 0.0);
		return QualityLevel.Rare;
	}
}
