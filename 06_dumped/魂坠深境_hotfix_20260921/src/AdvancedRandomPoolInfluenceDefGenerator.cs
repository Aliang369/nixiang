using System.Collections.Generic;
using GameRandom;
using UnityEngine;

public class AdvancedRandomPoolInfluenceDefGenerator : InfluenceDefGenerator
{
	[SerializeField]
	private ValueGenerator countGenerator;

	[SerializeField]
	private bool limitQuality;

	[SerializeField]
	private List<QualityLevel> allowedQualities;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		int num = 1;
		if (countGenerator != null)
		{
			int? num2 = countGenerator.GenerateValue(eve);
			if (num2.HasValue)
			{
				num = num2.Value;
			}
		}
		if (num <= 0)
		{
			return new List<InfluenceDef>();
		}
		List<InfluenceDef> list = eve.gameState.generationManager.GetAllGenerateAbleBless();
		if (limitQuality && allowedQualities != null && allowedQualities.Count > 0)
		{
			list = list.FindAll((InfluenceDef b) => allowedQualities.Contains(b.quality));
		}
		List<InfluenceDef> list2 = list.SelectRandomWeightCountUnique(BlessWeightGenerator, num, eve.gameState.randomManager.NextDouble);
		list2.ForEach(delegate(InfluenceDef inf)
		{
			eve.gameState.recordManager.generatedInfluence.AddWithoutDuplicate(inf);
		});
		return list2;
		float BlessWeightGenerator(InfluenceDef bless)
		{
			float num3 = 0f;
			num3 = bless.quality switch
			{
				QualityLevel.Common => 30f, 
				QualityLevel.Uncommon => 30f, 
				QualityLevel.Rare => 20f, 
				_ => 10f, 
			};
			if (eve.gameState.recordManager.generatedInfluence.Contains(bless))
			{
				num3 *= 0.5f;
			}
			if (eve.gameState.playerState.bft.influences.Find((Influence t) => t.def == bless) != null)
			{
				num3 = 0f;
			}
			return num3;
		}
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		if (limitQuality && allowedQualities != null && allowedQualities.Count > 0)
		{
			nodeDisplayInfo.qualityLevel = new NodeDisplayInfo.Element<QualityLevel>(allowedQualities[0]);
		}
		return nodeDisplayInfo;
	}

	public override List<IEffectNode> GetChildren()
	{
		List<IEffectNode> list = new List<IEffectNode>();
		if (countGenerator != null)
		{
			list.Add(countGenerator);
		}
		return list;
	}
}
