using System.Collections.Generic;

public class FirepowerSourcePowerResolveAction : ExecuteAction
{
	private readonly FirepowerSourcePowerCastPlan castPlan;

	private readonly bool consumeTargetCostWhenTriggered;

	public FirepowerSourcePowerResolveAction(FirepowerSourcePowerCastPlan castPlan, bool consumeTargetCostWhenTriggered)
	{
		this.castPlan = castPlan;
		this.consumeTargetCostWhenTriggered = consumeTargetCostWhenTriggered;
	}

	protected override void Update()
	{
		if (!castPlan.CanResolveSwap())
		{
			castPlan.Cancel();
			return;
		}
		bool flag = castPlan.CanPrepareTrigger() && castPlan.costPrepared && castPlan.targetPrepared && castPlan.requirementPrepared;
		DisplaySpellCastInfo displaySpellCastInfo = null;
		if (flag)
		{
			displaySpellCastInfo = castPlan.CreateDisplayCastInfo(gameState);
			flag = castPlan.ApplyCosts(displaySpellCastInfo, gameState);
		}
		if (flag)
		{
			displaySpellCastInfo.targetItem = castPlan.spellTargetItem;
			int finalCost = castPlan.GetFinalCost(displaySpellCastInfo);
			flag = castPlan.IsCostFulfilled(gameState, displaySpellCastInfo, finalCost) && displaySpellCastInfo.SpellCastAble(out var _);
		}
		castPlan.state = FirepowerSourcePowerCastPlan.State.Confirmed;
		EnqueueAction(new ChangeEnchantAction(castPlan.sourceItem, castPlan.sourceItem, castPlan.sourcePowerEnchant, -1));
		EnqueueAction(new SwapSlotItemAction(castPlan.sourceItem, castPlan.targetItem, castPlan.sourceItem));
		if (flag)
		{
			List<Item> targetCosts = new List<Item>(castPlan.targetCosts);
			List<Item> preSelectedItems = new List<Item>(castPlan.preSelectedItems);
			EnqueueAction(new ActionAction(delegate
			{
				RealSpellCastInfo.TriggerSpellCast(gameState, castPlan.targetItem, castPlan.spellTargetItem, SpellCastInfo.CasterType.Effect, triggerCast: true, targetCosts, castPlan.targetSpell, null, suppressRemoveAfterCast: false, consumeTargetCostWhenTriggered, triggerFromProtectedExile: false, preSelectedItems);
			}));
		}
	}
}
