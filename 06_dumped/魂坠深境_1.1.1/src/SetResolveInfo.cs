using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SetResolveInfo : Effect
{
	public enum SetType
	{
		Reject,
		Delta,
		DeltaPercent,
		SourceDelta,
		SourceDeltaPercent,
		TargetDelta,
		TargetDeltaPercent,
		FilteredItemDeltaValue,
		UpperBoundary,
		LowerBoundary,
		NoOverflow,
		DeltaTimes,
		OverrideBfttarget,
		FilteredItemSetValue,
		Pierce,
		Promote,
		FixedTargetValue,
		ReplaceItem,
		RealHealthOnly
	}

	public SetType type;

	public CheckNode boolValueGenerator;

	public CheckNode itemCheckNode;

	public GameEvent.EventBFTType overrideBft;

	public ValueGenerator vg;

	public ItemTargetSelector itemTargetSelector;

	private bool ShowBoolValueGenerator
	{
		get
		{
			if (type != SetType.Reject && type != SetType.NoOverflow && type != SetType.Pierce && type != SetType.Promote)
			{
				return type == SetType.RealHealthOnly;
			}
			return true;
		}
	}

	private bool ShowCheckNode
	{
		get
		{
			if (type != SetType.FilteredItemDeltaValue)
			{
				return type == SetType.FilteredItemSetValue;
			}
			return true;
		}
	}

	private bool ShowOverrideBft => type == SetType.OverrideBfttarget;

	private bool ShowItemTargetSelector => type == SetType.ReplaceItem;

	private bool ShowValueGenerator
	{
		get
		{
			if (!ShowItemTargetSelector && !ShowOverrideBft && !ShowCheckNode)
			{
				return !ShowBoolValueGenerator;
			}
			return false;
		}
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.resolveInfo == null)
		{
			return NodeCastResult.Failure;
		}
		int valueOrDefault = (vg?.GenerateValue(eve)).GetValueOrDefault();
		bool valueOrDefault2 = boolValueGenerator?.Check(eve) == true;
		List<Item> list = itemTargetSelector?.GetTargetItems(eve);
		switch (type)
		{
		case SetType.Reject:
			if (!eve.resolveInfo.reject & valueOrDefault2)
			{
				eve.resolveInfo.reject = valueOrDefault2;
				eve.resolveInfo.rejectSource = eve.CasterItem;
			}
			break;
		case SetType.Delta:
			eve.resolveInfo.deltaValue += valueOrDefault;
			break;
		case SetType.DeltaPercent:
			eve.resolveInfo.deltaPercent += valueOrDefault;
			break;
		case SetType.SourceDelta:
			eve.resolveInfo.sourceDelta += valueOrDefault;
			break;
		case SetType.SourceDeltaPercent:
			eve.resolveInfo.sourceDeltaPercent += valueOrDefault;
			break;
		case SetType.TargetDelta:
			eve.resolveInfo.targetDelta += valueOrDefault;
			break;
		case SetType.TargetDeltaPercent:
			eve.resolveInfo.targetDeltaPercent += valueOrDefault;
			break;
		case SetType.FilteredItemDeltaValue:
		{
			for (int j = 0; j < eve.resolveInfo.itemValuePairs.Count; j++)
			{
				(Item, int) tuple3 = eve.resolveInfo.itemValuePairs[j];
				Item nodeTempItem2 = eve.NodeTempItem;
				(eve.NodeTempItem, _) = tuple3;
				if (itemCheckNode == null || itemCheckNode.Check(eve) == true)
				{
					tuple3.Item2 += valueOrDefault;
				}
				eve.NodeTempItem = nodeTempItem2;
				eve.resolveInfo.itemValuePairs[j] = tuple3;
			}
			break;
		}
		case SetType.UpperBoundary:
			eve.resolveInfo.upperBoundary = Mathf.Min(valueOrDefault, eve.resolveInfo.upperBoundary);
			break;
		case SetType.LowerBoundary:
			eve.resolveInfo.lowerBoundary = Mathf.Max(valueOrDefault, eve.resolveInfo.lowerBoundary);
			break;
		case SetType.NoOverflow:
			eve.resolveInfo.noOverflow |= valueOrDefault2;
			break;
		case SetType.Pierce:
			eve.resolveInfo.pierce |= valueOrDefault2;
			break;
		case SetType.DeltaTimes:
			eve.resolveInfo.deltaTimes += valueOrDefault;
			break;
		case SetType.OverrideBfttarget:
			eve.resolveInfo.overrideBftTarget = eve.GetEventBFT(overrideBft);
			break;
		case SetType.FilteredItemSetValue:
		{
			for (int i = 0; i < eve.resolveInfo.itemValuePairs.Count; i++)
			{
				(Item, int) tuple = eve.resolveInfo.itemValuePairs[i];
				Item nodeTempItem = eve.NodeTempItem;
				(eve.NodeTempItem, _) = tuple;
				if (itemCheckNode == null || itemCheckNode.Check(eve) == true)
				{
					tuple.Item2 = valueOrDefault;
				}
				eve.NodeTempItem = nodeTempItem;
				eve.resolveInfo.itemValuePairs[i] = tuple;
			}
			break;
		}
		case SetType.Promote:
			eve.resolveInfo.promote = valueOrDefault2;
			if (valueOrDefault2 && eve.GetEveObj(GameEvent.EveObjType.Caster) is IGameEventSubscriber item)
			{
				eve.resolveInfo.promoteSources.Add(item);
			}
			break;
		case SetType.FixedTargetValue:
			eve.resolveInfo.fixedTargetValue = valueOrDefault;
			eve.resolveInfo.isFixedTargetValue = true;
			break;
		case SetType.ReplaceItem:
			eve.resolveInfo.replaceItem = list?.FirstOrDefault();
			break;
		case SetType.RealHealthOnly:
			eve.resolveInfo.realHealthOnly |= valueOrDefault2;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { vg, itemCheckNode, boolValueGenerator, itemTargetSelector };
	}
}
