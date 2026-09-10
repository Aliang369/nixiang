using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;
using UnityEngine;

public class WeightedPlayerCurrentInfluenceDefGenerator : InfluenceDefGenerator
{
	[SerializeField]
	[Min(0f)]
	private int count = 3;

	[SerializeField]
	[Min(0f)]
	private int rootBlessingWeight = 3;

	[SerializeField]
	[Min(0f)]
	private int commonBlessingWeight = 2;

	[SerializeField]
	[Min(0f)]
	private int riftBlessingWeight = 1;

	public override List<InfluenceDef> GenerateInfluenceDefs(GameEvent eve)
	{
		GameStateInfo gameStateInfo = eve?.gameState;
		List<Influence> source = gameStateInfo?.playerState?.bft?.influences;
		if (source.IsNullOrEmpty() || gameStateInfo.randomManager == null)
		{
			return null;
		}
		List<InfluenceDef> list = SelectWeightedUnique(source.Select((Influence influence) => influence?.def), GetRiftBlessingDefs(gameStateInfo.applicationState), count, rootBlessingWeight, commonBlessingWeight, riftBlessingWeight, gameStateInfo.randomManager.NextDouble);
		if (!list.IsNullOrEmpty())
		{
			return list;
		}
		return null;
	}

	public static HashSet<InfluenceDef> GetRiftBlessingDefs(ApplicationStateInfo applicationState)
	{
		List<WorldEvent> list = applicationState?.assetCatalog?.worldEventCatalog?.ValidAssets;
		if (list == null)
		{
			return new HashSet<InfluenceDef>();
		}
		return (from config in list.OfType<RiftWorldEvent>().SelectMany((RiftWorldEvent worldEvent) => worldEvent.blessings ?? new List<RiftCharacterBlessingConfig>())
			select config?.blessing into def
			where def != null
			select def).ToHashSet();
	}

	public static int GetBlessingWeight(InfluenceDef def, ISet<InfluenceDef> riftBlessingDefs, int rootWeight, int commonWeight, int riftWeight)
	{
		if (def == null || def.type != InfluenceDef.InfluenceType.Positive || def.hideInIndex)
		{
			return 0;
		}
		if (def.rootBonfireBlessPool)
		{
			return Mathf.Max(0, rootWeight);
		}
		if (riftBlessingDefs != null && riftBlessingDefs.Contains(def))
		{
			return Mathf.Max(0, riftWeight);
		}
		return Mathf.Max(0, commonWeight);
	}

	public static List<InfluenceDef> SelectWeightedUnique(IEnumerable<InfluenceDef> influenceDefs, ISet<InfluenceDef> riftBlessingDefs, int count, int rootWeight, int commonWeight, int riftWeight, Func<double> nextRandomGetter)
	{
		if (influenceDefs == null || nextRandomGetter == null || count <= 0)
		{
			return new List<InfluenceDef>();
		}
		List<InfluenceDef> list = (from def in influenceDefs.Where((InfluenceDef def) => def != null).Distinct()
			where GetBlessingWeight(def, riftBlessingDefs, rootWeight, commonWeight, riftWeight) > 0
			select def).ToList();
		if (list.Count == 0)
		{
			return new List<InfluenceDef>();
		}
		List<float> weights = ((IEnumerable<InfluenceDef>)list).Select((Func<InfluenceDef, float>)((InfluenceDef def) => GetBlessingWeight(def, riftBlessingDefs, rootWeight, commonWeight, riftWeight))).ToList();
		return list.SelectRandomWeightCountUnique(weights, Mathf.Min(count, list.Count), nextRandomGetter);
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(count)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode>();
	}
}
