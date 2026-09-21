using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QualityInfluenceDefGenerator : InfluenceDefGenerator
{
	[SerializeField]
	private bool blessing;

	[SerializeField]
	private QualityLevel quality;

	[SerializeField]
	private bool excludeExisting;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		List<InfluenceDef> list = new List<InfluenceDef>();
		list = eve.gameState.applicationState.assetCatalog.influenceCatalog.ValidAssets;
		if (blessing)
		{
			list = eve.gameState.applicationState.assetCatalog.influenceCatalog.ValidAssets.FindAll((InfluenceDef def) => def.quality == quality && def.randomGenerate);
		}
		if (excludeExisting)
		{
			list = list.Where((InfluenceDef infD) => !eve.gameState.playerState.bft.influences.Any((Influence i) => i.def == infD)).ToList();
		}
		return list;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			qualityLevel = new NodeDisplayInfo.Element<QualityLevel>(quality)
		};
	}
}
