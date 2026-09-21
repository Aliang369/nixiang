using System.Collections.Generic;

public class FirepowerSourcePowerCostAction : ExecuteAction
{
	private readonly FirepowerSourcePowerCastPlan castPlan;

	public FirepowerSourcePowerCostAction(FirepowerSourcePowerCastPlan castPlan)
	{
		this.castPlan = castPlan;
	}

	protected override void Update()
	{
		DisplaySpellCastInfo castInfo;
		int finalCost;
		bool freelySelectCost;
		List<Item> selectedCosts;
		if (!castPlan.CanResolveSwap())
		{
			castPlan.Cancel();
		}
		else
		{
			if (!castPlan.CanPrepareTrigger())
			{
				return;
			}
			castInfo = castPlan.CreateDisplayCastInfo(gameState);
			if (castInfo == null)
			{
				castPlan.Cancel();
				return;
			}
			finalCost = castPlan.GetFinalCost(castInfo);
			bool flag = castPlan.targetItem.IsManualSelectCost(gameState, GameEvent.TriggerType.InBattleDisplay);
			freelySelectCost = castPlan.targetSpell.ConsumesAllAvailableCost;
			if (flag && finalCost <= 0 && !freelySelectCost)
			{
				castPlan.costPrepared = true;
				return;
			}
			if (!flag)
			{
				castInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
				if (!castPlan.IsCostFulfilled(gameState, castInfo, finalCost))
				{
					castPlan.SkipTrigger();
					return;
				}
				castPlan.targetCosts = new List<Item>(castInfo.addedCosts);
				castPlan.costPrepared = true;
				return;
			}
			List<Item> list = castPlan.targetItem.owner?.GetAllItems();
			if (list == null)
			{
				castPlan.SkipTrigger();
				return;
			}
			selectedCosts = new List<Item>();
			SelectItemInfo info = SelectItemInfo.GenerateSelectItemsInfo(castInfo.GenerateSpellCastEvent(GameEvent.TriggerType.InBattleDisplay), list, () => false, CanSelect, CanConfirm, OnConfirm, castPlan.SkipTrigger, () => true, castPlan.targetItem.OwnerBft, () => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_item_as_cost, isSingleSelect: false, selectedCosts, castPlan.targetItem);
			EnqueueAction(new SelectItemAction(info, PlayerSelectItemsEffect.SelectPanelType.Table));
		}
		bool CanConfirm()
		{
			if (!freelySelectCost)
			{
				return SelectedCostValue() >= finalCost;
			}
			return true;
		}
		bool CanSelect(Item item)
		{
			if (item == null || item == castPlan.targetItem || selectedCosts.Contains(item))
			{
				return false;
			}
			if (!freelySelectCost && SelectedCostValue() >= finalCost)
			{
				return false;
			}
			if (!gameState.PlayerControlAble || !castInfo.IsFocusSpellCastTimingAble(GameEvent.TriggerType.InBattleDisplay))
			{
				return false;
			}
			if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ForbidAddCost))
			{
				return false;
			}
			return !gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(item, castInfo, GameEvent.TriggerType.InBattleDisplay).reject;
		}
		void OnConfirm()
		{
			castInfo.ReturnAllCost();
			foreach (Item item in selectedCosts)
			{
				castInfo.AddCostItem(item, GameEvent.TriggerType.InBattleDisplay);
			}
			if (!castPlan.IsCostFulfilled(gameState, castInfo, finalCost))
			{
				castPlan.SkipTrigger();
			}
			else
			{
				castPlan.targetCosts = new List<Item>(castInfo.addedCosts);
				castPlan.costPrepared = true;
			}
		}
		int SelectedCostValue()
		{
			int num = 0;
			foreach (Item item2 in selectedCosts)
			{
				GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(item2, castInfo, GameEvent.TriggerType.InBattleDisplay);
				if (!resolveInfo.reject)
				{
					num += 1 + resolveInfo.deltaValue;
				}
			}
			return num;
		}
	}
}
