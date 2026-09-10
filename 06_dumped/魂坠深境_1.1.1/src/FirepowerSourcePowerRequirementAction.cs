using System.Collections;
using System.Collections.Generic;
using ContainerOfSoul.TargetRequirement;
using UnityEngine;

public class FirepowerSourcePowerRequirementAction : AbstractEnumeratorAction
{
	private readonly FirepowerSourcePowerCastPlan castPlan;

	public FirepowerSourcePowerRequirementAction(FirepowerSourcePowerCastPlan castPlan)
	{
		this.castPlan = castPlan;
	}

	protected override IEnumerator Run()
	{
		if (!castPlan.CanResolveSwap())
		{
			castPlan.Cancel();
		}
		else
		{
			if (!castPlan.CanPrepareTrigger() || !castPlan.costPrepared || !castPlan.targetPrepared)
			{
				yield break;
			}
			DisplaySpellCastInfo displaySpellCastInfo = castPlan.CreateDisplayCastInfo(gameState);
			if (!castPlan.ApplyCosts(displaySpellCastInfo, gameState))
			{
				castPlan.SkipTrigger();
				yield break;
			}
			displaySpellCastInfo.targetItem = castPlan.spellTargetItem;
			CastPrepare castPrepare = castPlan.targetSpell.requirement?.targetRequirement;
			if (castPrepare == null)
			{
				castPlan.requirementPrepared = true;
				yield break;
			}
			bool finished = false;
			GameEvent prepareEvent = displaySpellCastInfo.GenerateSelectItemPreCastEvent();
			castPrepare.TriggerPrepare(prepareEvent, delegate
			{
				castPlan.preSelectedItems = new List<Item>(prepareEvent.SelectedItems ?? new List<Item>());
				if (prepareEvent.TargetItem != null)
				{
					castPlan.spellTargetItem = prepareEvent.TargetItem;
				}
				castPlan.requirementPrepared = true;
				finished = true;
			}, delegate
			{
				castPlan.SkipTrigger();
				finished = true;
			});
			yield return new WaitUntil(() => finished);
		}
	}
}
