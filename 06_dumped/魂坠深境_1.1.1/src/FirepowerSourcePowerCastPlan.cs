using System.Collections.Generic;
using System.Linq;

public class FirepowerSourcePowerCastPlan
{
	public enum State
	{
		Pending,
		TriggerUnavailable,
		Confirmed,
		Cancelled
	}

	public readonly EnchantDef sourcePowerEnchant;

	public readonly Item sourceItem;

	public readonly Item targetItem;

	public readonly ItemSpell targetSpell;

	public readonly bool allowAddedCostAsTarget;

	public bool costPrepared;

	public bool requirementPrepared;

	public State state;

	public Item spellTargetItem;

	public bool targetPrepared;

	public List<Item> targetCosts = new List<Item>();

	public List<Item> preSelectedItems = new List<Item>();

	public FirepowerSourcePowerCastPlan(Item sourceItem, Item targetItem, ItemSpell targetSpell, EnchantDef sourcePowerEnchant, bool consumeTargetCostWhenTriggered = true)
	{
		this.sourceItem = sourceItem;
		this.targetItem = targetItem;
		this.targetSpell = targetSpell;
		this.sourcePowerEnchant = sourcePowerEnchant;
		allowAddedCostAsTarget = !consumeTargetCostWhenTriggered;
	}

	public bool CanContinue()
	{
		return CanPrepareTrigger();
	}

	public bool CanResolveSwap()
	{
		if (state != State.Confirmed && state != State.Cancelled && sourceItem != null && targetItem != null && targetSpell != null && sourcePowerEnchant != null && sourceItem.owner != null && sourceItem.owner == targetItem.owner && targetItem.BaseSpell == targetSpell)
		{
			return sourceItem.Enchants.Any((Enchant enchant) => enchant.def == sourcePowerEnchant && (int)enchant.value > 0);
		}
		return false;
	}

	public bool CanPrepareTrigger()
	{
		if (state == State.Pending)
		{
			return CanResolveSwap();
		}
		return false;
	}

	public void SkipTrigger()
	{
		if (state == State.Pending)
		{
			state = State.TriggerUnavailable;
		}
	}

	public void Cancel()
	{
		if (state != State.Confirmed)
		{
			state = State.Cancelled;
		}
	}

	public DisplaySpellCastInfo CreateDisplayCastInfo(GameStateInfo gameState)
	{
		if (!CanContinue())
		{
			return null;
		}
		return new DisplaySpellCastInfo(gameState, inBattleDisplay: true)
		{
			spellItem = targetItem,
			forcedFocusSpell = targetSpell,
			casterType = SpellCastInfo.CasterType.Player,
			isTriggerCast = true,
			allowAddedCostAsTarget = allowAddedCostAsTarget
		};
	}

	public bool ApplyCosts(DisplaySpellCastInfo castInfo, GameStateInfo gameState)
	{
		if (castInfo == null)
		{
			return false;
		}
		castInfo.ReturnAllCost();
		foreach (Item targetCost in targetCosts)
		{
			castInfo.AddCostItem(targetCost, GameEvent.TriggerType.InBattleDisplay);
		}
		return castInfo.addedCosts.Count == targetCosts.Count;
	}

	public int GetFinalCost(DisplaySpellCastInfo castInfo)
	{
		return targetSpell.requirement?.GetFinalCostValue(castInfo, GameEvent.TriggerType.InBattleDisplay) ?? 0;
	}

	public bool IsCostFulfilled(GameStateInfo gameState, DisplaySpellCastInfo castInfo, int finalCost)
	{
		if (finalCost > 0 && castInfo.addedCostValue < finalCost)
		{
			return gameState.gameEventManager.TriggerCanCastWithoutFulfillCostResolve(castInfo).promote;
		}
		return true;
	}
}
