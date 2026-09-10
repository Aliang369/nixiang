using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FixedContractRoute", menuName = "Contract/Fixed Contract Route")]
public sealed class FixedContractRouteDef : ScriptableObject
{
	[Serializable]
	public sealed class StageRoute
	{
		public int stage;

		public Level level;

		public DraftMapRef map;

		public List<FixedNode> nodes = new List<FixedNode>();

		public bool HasFixedNodes
		{
			get
			{
				if (nodes != null)
				{
					return nodes.Count > 0;
				}
				return false;
			}
		}

		public FixedNode GetNode(int draftMapIndex)
		{
			return nodes?.FirstOrDefault((FixedNode node) => node != null && node.draftMapIndex == draftMapIndex);
		}
	}

	[Serializable]
	public sealed class FixedNode
	{
		public string nodeId;

		public string sourceName;

		public int draftMapIndex;

		public DraftPivot.PivotType sourcePivotType;

		public PathLocation.LocationType locationType;

		public RoomRef room;
	}

	public string routeId;

	[Min(1f)]
	public int version = 1;

	public ContractDef contract;

	public List<StageRoute> stages = new List<StageRoute>();

	public int FinalStage => stages?.Where((StageRoute route) => route != null).Select((StageRoute route) => route.stage).DefaultIfEmpty(0)
		.Max() ?? 0;

	public StageRoute GetStage(int stage)
	{
		return stages?.FirstOrDefault((StageRoute route) => route != null && route.stage == stage);
	}

	public static int ResolveCompletionStage(FixedContractRouteDef route, int defaultStage)
	{
		int num = route?.FinalStage ?? 0;
		if (num <= 0)
		{
			return defaultStage;
		}
		return num;
	}

	public bool IsCompletedAfterStage(int stage)
	{
		if (FinalStage > 0)
		{
			return stage >= FinalStage;
		}
		return false;
	}

	public IReadOnlyList<string> GetValidationErrors()
	{
		List<string> list = new List<string>();
		if (string.IsNullOrWhiteSpace(routeId))
		{
			list.Add("routeId is required.");
		}
		if (version < 1)
		{
			list.Add("version must be at least 1.");
		}
		if (contract == null)
		{
			list.Add("contract is required.");
		}
		if (stages == null || stages.Count == 0)
		{
			list.Add("At least one stage route is required.");
			return list;
		}
		foreach (IGrouping<int, StageRoute> item in from route in stages
			where route != null
			group route by route.stage into @group
			where @group.Count() > 1
			select @group)
		{
			list.Add($"Stage {item.Key} is configured more than once.");
		}
		foreach (StageRoute stage in stages)
		{
			if (stage == null)
			{
				list.Add("Stage route cannot be null.");
				continue;
			}
			if (stage.stage < 1)
			{
				list.Add($"Stage {stage.stage} must be at least 1.");
			}
			if (stage.level == null)
			{
				list.Add($"Stage {stage.stage} requires a Level.");
			}
			else if (stage.level.stage != stage.stage)
			{
				list.Add($"Stage {stage.stage} references Level {stage.level.Name} with stage {stage.level.stage}.");
			}
			if (stage.map == null || string.IsNullOrWhiteSpace(stage.map.CTAssetId))
			{
				list.Add($"Stage {stage.stage} requires a DraftMapRef.");
			}
			if (!stage.HasFixedNodes)
			{
				continue;
			}
			foreach (IGrouping<int, FixedNode> item2 in from node in stage.nodes
				where node != null
				group node by node.draftMapIndex into @group
				where @group.Count() > 1
				select @group)
			{
				list.Add($"Stage {stage.stage} configures map node index {item2.Key} more than once.");
			}
			foreach (IGrouping<string, FixedNode> item3 in from node in stage.nodes
				where node != null && !string.IsNullOrWhiteSpace(node.nodeId)
				group node by node.nodeId into @group
				where @group.Count() > 1
				select @group)
			{
				list.Add($"Stage {stage.stage} configures node ID {item3.Key} more than once.");
			}
			foreach (FixedNode node in stage.nodes)
			{
				if (node == null)
				{
					list.Add($"Stage {stage.stage} contains a null node.");
					continue;
				}
				if (string.IsNullOrWhiteSpace(node.nodeId))
				{
					list.Add($"Stage {stage.stage} node {node.draftMapIndex} requires a stable node ID.");
				}
				if (node.draftMapIndex < 0)
				{
					list.Add($"Stage {stage.stage} node {node.nodeId} has an invalid map index.");
				}
				if (node.locationType == PathLocation.LocationType.None)
				{
					list.Add($"Stage {stage.stage} node {node.nodeId} requires a fixed location type.");
				}
				if (node.room == null || string.IsNullOrWhiteSpace(node.room.CTAssetId))
				{
					list.Add($"Stage {stage.stage} node {node.nodeId} requires a fixed room.");
				}
			}
		}
		return list;
	}
}
