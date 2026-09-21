using System.Collections.Generic;
using UnityEngine;

public class ChangeStance : Effect
{
	public StanceDefSelector stanceDefSelector;

	public ValueGenerator valueGenerator;

	public bool changeStanceMaxValue;

	public bool changeMultiStance;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return NodeCastResult.Failure;
		}
		if (changeStanceMaxValue)
		{
			int? num = valueGenerator?.GenerateValue(eve) + eve.gameState.stanceManager?.stanceMaxValue;
			if (!num.HasValue)
			{
				return NodeCastResult.Failure;
			}
			Effect.EnqueueAction(eve, new SetMaxStanceAction(num.Value));
		}
		else if (changeMultiStance)
		{
			List<Stance> list = GenerateDeltaStances(eve);
			if (list == null || list.Count == 0)
			{
				return NodeCastResult.Failure;
			}
			foreach (Stance item in list)
			{
				Effect.EnqueueAction(eve, new ChangeStanceAction(item, eve.Caster));
			}
		}
		else
		{
			Stance deltaStance = GenerateDeltaStance(eve);
			Effect.EnqueueAction(eve, new ChangeStanceAction(deltaStance, eve.Caster));
		}
		return NodeCastResult.Success;
	}

	public Stance GenerateDeltaStance(GameEvent eve)
	{
		StanceDef stanceDef = stanceDefSelector.GenerateSingleStanceDef(eve);
		int? num = valueGenerator?.GenerateValue(eve);
		if (stanceDef == null || !num.HasValue)
		{
			return null;
		}
		Stance stance = new Stance(stanceDef, num.Value, eve.gameState?.playerState?.bft);
		if (eve.gameState?.gameEventManager != null)
		{
			GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerChangeStanceValueResolve(stance, eve.Caster, GameEvent.TriggerType.InBattleDisplay);
			stance.value += resolveInfo.deltaValue;
			stance.value = Mathf.Min(stance.value, resolveInfo.upperBoundary);
			stance.value = Mathf.Max(stance.value, resolveInfo.lowerBoundary);
			if (resolveInfo.reject)
			{
				return null;
			}
		}
		return stance;
	}

	public List<Stance> GenerateDeltaStances(GameEvent eve)
	{
		List<StanceDef> list = stanceDefSelector.GenerateStanceDefs(eve);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		List<Stance> list2 = new List<Stance>();
		int? num = valueGenerator?.GenerateValue(eve);
		if (!num.HasValue)
		{
			return list2;
		}
		foreach (StanceDef item in list)
		{
			Stance stance = new Stance(item, num.Value, eve.gameState?.playerState?.bft);
			if (eve.gameState?.gameEventManager != null)
			{
				GameEvent.ResolveInfo resolveInfo = eve.gameState.gameEventManager.TriggerChangeStanceValueResolve(stance, eve.Caster, GameEvent.TriggerType.InBattleDisplay);
				stance.value += resolveInfo.deltaValue;
				stance.value = Mathf.Min(stance.value, resolveInfo.upperBoundary);
				stance.value = Mathf.Max(stance.value, resolveInfo.lowerBoundary);
			}
			list2.Add(stance);
		}
		return list2;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		NodeDisplayInfo nodeDisplayInfo = new NodeDisplayInfo();
		int? num = valueGenerator?.GenerateValue(eve);
		Stance stance = GenerateDeltaStance(eve);
		if (stance != null)
		{
			nodeDisplayInfo.refStance = new NodeDisplayInfo.Element<Stance>(stance);
			nodeDisplayInfo.deltaValue = new NodeDisplayInfo.Element<int?>(stance.value - num);
		}
		return nodeDisplayInfo;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { stanceDefSelector, valueGenerator };
	}
}
