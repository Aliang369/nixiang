using UnityEngine;

public class ChangeStanceAction : GameAction
{
	public Stance deltaStance;

	public object source;

	public ChangeStanceAction(Stance deltaStance, object source)
	{
		this.deltaStance = deltaStance;
		this.source = source;
	}

	protected override void Update()
	{
		if (deltaStance?.def == null)
		{
			FinishAction();
			return;
		}
		if (deltaStance.value == 0)
		{
			FinishAction();
			return;
		}
		Stance currentStance = gameState.stanceManager.currentStance;
		Stance stance = Stance.Cast(currentStance, deltaStance, out var changeType);
		PrepareChangeStance(currentStance, stance, changeType);
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerChangeStanceValueWithChangeTypeResolve(deltaStance, source, GameEvent.TriggerType.Real, changeType);
		if (resolveInfo != null)
		{
			stance.value += resolveInfo.deltaValue;
			stance.value = Mathf.Min(stance.value, resolveInfo.upperBoundary);
			stance.value = Mathf.Max(stance.value, resolveInfo.lowerBoundary);
		}
		ChangeStance(currentStance, stance, changeType);
		FinishAction();
	}

	private Stance.ChangeType? GetChangeType(Stance current, Stance delta)
	{
		if (current == null || current.value <= 0)
		{
			if (delta.value <= 0)
			{
				return null;
			}
			return Stance.ChangeType.None;
		}
		if (current.def == delta.def)
		{
			Stance stance = Stance.Add(current, deltaStance);
			if (stance == null || stance.value <= 0)
			{
				return Stance.ChangeType.Break;
			}
			if (deltaStance.value > 0)
			{
				return Stance.ChangeType.Stack;
			}
			return Stance.ChangeType.None;
		}
		return Stance.ChangeType.Shift;
	}

	private void AddStance()
	{
		Stance currentStance = gameState.stanceManager.currentStance;
		Stance stance = Stance.Add(currentStance, deltaStance);
		if (stance == null || stance.value <= 0)
		{
			ChangeStance(currentStance, null, Stance.ChangeType.Break);
		}
		else if (deltaStance.value > 0)
		{
			ChangeStance(currentStance, stance, Stance.ChangeType.Stack);
		}
		else
		{
			ChangeStance(currentStance, stance, Stance.ChangeType.None);
		}
	}

	private void PrepareChangeStance(Stance before, Stance after, Stance.ChangeType stanceChangeType)
	{
		gameState.gameEventManager.TriggerBeforeChangeStanceValue(before, after, deltaStance, source, stanceChangeType.ToString());
		if (gameState.gameEventManager.TriggerRepeatStanceAbilityEffectResolve().promote)
		{
			gameState.stanceManager.NeedRepeatTrigger = true;
			gameState.gameEventManager.TriggerBeforeChangeStanceValue(before, after, deltaStance, source, stanceChangeType.ToString());
		}
	}

	private void ChangeStance(Stance before, Stance after, Stance.ChangeType stanceChangeType)
	{
		if (after.value <= 0)
		{
			after = null;
		}
		gameState.stanceManager.currentStance = after;
		EnqueueAction(new CheckStanceMaxAction(after, source));
		EnqueueAction(delegate
		{
			gameState.gameEventManager.TriggerAfterChangeStanceValue(before, after, deltaStance, source, stanceChangeType.ToString());
			if (gameState.stanceManager.NeedRepeatTrigger)
			{
				gameState.gameEventManager.TriggerAfterChangeStanceValue(before, after, deltaStance, source, stanceChangeType.ToString());
			}
			gameState.stanceManager.NeedRepeatTrigger = false;
		});
	}
}
