using System.Collections.Generic;

public class PoolGenerateBless : InfluenceDefGenerator
{
	public ValueGenerator countGenerator;

	public QualityLevel qualityLevel;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		if (eve.triggerType != GameEvent.TriggerType.Real)
		{
			return null;
		}
		int count = countGenerator.GenerateValue(eve) ?? 1;
		return eve.gameState.generationManager.GenerateRandomCommonBless(count, updateRecord: true, qualityLevel);
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { countGenerator };
	}
}
