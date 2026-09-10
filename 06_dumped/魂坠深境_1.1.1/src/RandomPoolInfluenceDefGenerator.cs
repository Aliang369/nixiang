using System.Collections.Generic;

public class RandomPoolInfluenceDefGenerator : InfluenceDefGenerator
{
	public int count;

	public QualityLevel filterQuality;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		return eve.gameState.generationManager.GenerateRandomCommonBless(count, updateRecord: true, filterQuality);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(count),
			qualityLevel = new NodeDisplayInfo.Element<QualityLevel>(filterQuality)
		};
	}
}
