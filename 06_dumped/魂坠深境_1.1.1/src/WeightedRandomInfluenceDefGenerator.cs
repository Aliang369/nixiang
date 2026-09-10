using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using Sirenix.Serialization;
using UnityEngine;

public class WeightedRandomInfluenceDefGenerator : InfluenceDefGenerator
{
	[Serializable]
	public class InfluenceDefWeightDict
	{
		public InfluenceDef influenceDef;

		public int weight;

		public InfluenceDefWeightDict(InfluenceDef influenceDef, int weight)
		{
			this.influenceDef = influenceDef;
			this.weight = weight;
		}
	}

	[SerializeField]
	private int count = 1;

	[SerializeField]
	private bool useCurrentDifficultyLevel = true;

	[SerializeField]
	private DifficultyLevel level = DifficultyLevel.Normal;

	[OdinSerialize]
	private List<InfluenceDefWeightDict> manualInfluenceWeights = new List<InfluenceDefWeightDict>();

	private bool ShowManualWeights => !useCurrentDifficultyLevel;

	private List<InfluenceDefWeightDict> LoadWeightsFromDifficulty(DifficultyLevel difficulty)
	{
		if (ScriptableObjectSingleton<GameStatisticData>.instance == null || ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable == null)
		{
			return new List<InfluenceDefWeightDict>();
		}
		List<InfluenceDef> defs = ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable.Rows.OfType<InfluenceDef>().ToList();
		List<int> list = (from w in ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable.GetColumn(difficulty)
			select w.value).ToList();
		int num = defs.Count;
		int num2 = list.Count;
		int num3 = Math.Max(0, num2 - num);
		List<int> slicedWeights = list.Skip(num3).Take(num).ToList();
		return (from i in Enumerable.Range(0, num)
			select new InfluenceDefWeightDict(defs[i], slicedWeights[i])).ToList();
	}

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		List<InfluenceDef> defs = ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable.Columns.OfType<InfluenceDef>().ToList();
		List<int> list = (from w in ScriptableObjectSingleton<GameStatisticData>.instance.nightmareTable.GetRow(useCurrentDifficultyLevel ? eve.gameState.contractState.selectedContract.difficultyLevel : level)
			select w.value).ToList();
		int num = defs.Count;
		int num2 = list.Count;
		int num3 = Math.Max(0, num2 - num);
		List<int> slicedWeights = list.Skip(num3).Take(num).ToList();
		List<InfluenceDefWeightDict> list2 = (from i in Enumerable.Range(0, num)
			select new InfluenceDefWeightDict(defs[i], slicedWeights[i])).ToList();
		if (list2 == null || list2.Count == 0)
		{
			return null;
		}
		if (list2.Count == 0)
		{
			return null;
		}
		return (from x in list2.SelectRandomWeightCountUnique(((IEnumerable<InfluenceDefWeightDict>)list2).Select((Func<InfluenceDefWeightDict, float>)((InfluenceDefWeightDict x) => x.weight)).ToList(), 1)
			select x.influenceDef).ToList();
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo();
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
