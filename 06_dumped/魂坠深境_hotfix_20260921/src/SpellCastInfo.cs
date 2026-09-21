using System;
using System.Collections.Generic;

[Serializable]
public abstract class SpellCastInfo
{
	public enum SpellCastFailureReason
	{
		None = -1,
		NoFocusSpell = 1,
		NotControl = 2,
		NotEnoughCost = 3,
		NotCoolDowned = 4,
		ConditionFailure = 5,
		AlreadyAtHead = 6,
		ConsecutiveDazzlingAtHead = 7
	}

	public enum State
	{
		Preparing,
		PreCasting,
		Casting,
		FinishingCasting
	}

	public class AddedCostInfo
	{
		public int costValue;

		public Item item;
	}

	public enum CasterType
	{
		None,
		Player,
		Enemy,
		Effect
	}

	public List<AddedCostInfo> addedCostInfos = new List<AddedCostInfo>();

	public GameStateInfo gameState;

	public Item spellItem;

	public Item targetItem;

	public bool isTriggerCast;

	public bool allowAddedCostAsTarget;

	public List<Item> manualSelectedCosts = new List<Item>();

	public List<Item> precastSelectItems = new List<Item>();

	public Item triggerCastAnchorItem;

	public int realCostValue;

	public CasterType casterType;

	public ItemSpell forcedFocusSpell;

	public bool suppressRemoveAfterCast;

	public ItemSpell capturedFocusSpell;

	public Enchant capturedOverrideSpellEnchant;

	public bool triggerFromProtectedExile;

	public EnchantDefGenerator negativeEnchantReplacementGenerator;

	public bool suppressChangeEnchantAndSpawnItemEffects;

	public int? continuousDamageActionTriggerCount;

	public bool resourceDirectionReversalActive;

	public bool resourceDirectionReversalApplied;

	protected bool resourceDirectionReversalResolved;

	public ItemSpell focusSpell
	{
		get
		{
			ItemSpell currentSpell = capturedFocusSpell;
			if (currentSpell == null)
			{
				currentSpell = forcedFocusSpell;
				if (currentSpell == null)
				{
					Item item = spellItem;
					if (item == null)
					{
						return null;
					}
					currentSpell = item.CurrentSpell;
				}
			}
			return currentSpell;
		}
	}

	public int addCostCount => addedCostInfos.Count;

	public List<Item> addedCosts => addedCostInfos.ConvertAll((AddedCostInfo info) => info.item);

	public int addedCostValue
	{
		get
		{
			int num = 0;
			for (int i = 0; i < addedCostInfos.Count; i++)
			{
				num += addedCostInfos[i].costValue;
			}
			return num;
		}
	}

	protected bool hasFocusSpell => focusSpell != null;

	public bool NeedSpellTargetItem => requirement?.NeedTarget ?? false;

	protected ItemRequirement requirement => focusSpell?.requirement;

	public bool CheckItemAdded(Item item)
	{
		for (int i = 0; i < addedCostInfos.Count; i++)
		{
			if (addedCostInfos[i].item == item)
			{
				return true;
			}
		}
		return false;
	}

	public void AddCost(GameEvent.TriggerType triggerType)
	{
		if (casterType == CasterType.Enemy || casterType == CasterType.Effect)
		{
			return;
		}
		bool flag = spellItem.IsManualSelectCost(gameState, triggerType);
		if ((triggerType != GameEvent.TriggerType.Real) & flag)
		{
			return;
		}
		ReturnAllCost();
		if (requirement == null)
		{
			return;
		}
		ResolveResourceDirectionReversal(triggerType);
		List<Item> list = (flag ? manualSelectedCosts : spellItem.owner?.GetAllItems());
		if (list == null)
		{
			return;
		}
		int finalCostValue = requirement.GetFinalCostValue(this, triggerType);
		if (triggerType == GameEvent.TriggerType.Real)
		{
			realCostValue = finalCostValue;
		}
		ItemSpell itemSpell = focusSpell;
		if (itemSpell != null && itemSpell.MovesHeadToCasterRightAsCost)
		{
			return;
		}
		int? ownerSlotIndex = spellItem.ownerSlotIndex;
		if (!ownerSlotIndex.HasValue)
		{
			return;
		}
		ItemContainer.ItemSequenceState arg = null;
		if (triggerType == GameEvent.TriggerType.Real)
		{
			arg = new ItemContainer.ItemSequenceState(spellItem.owner);
		}
		bool variableFlag = gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.LeftHandednessAddCost);
		bool flag2 = resourceDirectionReversalActive && MeridianReversalAbility.HasReversibleAutomaticCost(this, triggerType);
		int num = ((!(variableFlag ^ flag2)) ? 1 : (-1));
		bool flag3 = focusSpell?.ConsumesAllAvailableCost ?? false;
		int num2 = 0;
		for (int i = ((!flag) ? ownerSlotIndex.Value : 0); i < list.Count && i >= 0; i += num)
		{
			if (flag || i != ownerSlotIndex.Value)
			{
				if (!flag3 && num2 >= finalCostValue)
				{
					break;
				}
				int count = addedCostInfos.Count;
				AddCostItem(list[i], triggerType);
				for (int j = count; j < addedCostInfos.Count; j++)
				{
					num2 += addedCostInfos[j].costValue;
				}
			}
		}
		if (triggerType == GameEvent.TriggerType.Real)
		{
			ItemContainer.ItemSequenceState arg2 = new ItemContainer.ItemSequenceState(spellItem.owner);
			spellItem.owner.itemSequenceChangeEvent?.Invoke(arg, arg2);
		}
	}

	public void AddCostItem(Item item, GameEvent.TriggerType triggerType)
	{
		bool num = hasFocusSpell && !CheckItemAdded(item) && gameState.PlayerControlAble && IsFocusSpellCastTimingAble(triggerType) && !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ForbidAddCost);
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(item, this, triggerType);
		if (num & !resolveInfo.reject)
		{
			AddedCostInfo item2 = new AddedCostInfo
			{
				item = item,
				costValue = 1 + resolveInfo.deltaValue
			};
			addedCostInfos.Add(item2);
		}
	}

	public void SetDefaultTargetItem()
	{
		if (focusSpell?.requirement != null)
		{
			targetItem = focusSpell.requirement.GetDefaultTargetItem(spellItem, gameState);
		}
	}

	public bool CheckItemAsTargetAble(Item item)
	{
		if (item == null || item.IsHidden(gameState))
		{
			return false;
		}
		if (focusSpell?.requirement != null && focusSpell.requirement.CheckTargetAccept(this, item))
		{
			if (!allowAddedCostAsTarget)
			{
				return !CheckItemAdded(item);
			}
			return true;
		}
		return false;
	}

	public virtual void ReturnAllCost()
	{
		addedCostInfos.Clear();
	}

	public void ResolveResourceDirectionReversal(GameEvent.TriggerType triggerType)
	{
		if (!resourceDirectionReversalResolved)
		{
			resourceDirectionReversalResolved = true;
			resourceDirectionReversalActive = MeridianReversalAbility.ShouldReverse(this, triggerType);
			resourceDirectionReversalApplied = resourceDirectionReversalActive;
		}
	}

	public void ResetResourceDirectionReversal()
	{
		resourceDirectionReversalResolved = false;
		resourceDirectionReversalActive = false;
		resourceDirectionReversalApplied = false;
	}

	public bool HasEnoughCost()
	{
		if (requirement == null)
		{
			return true;
		}
		int finalCostValue = requirement.GetFinalCostValue(this, GameEvent.TriggerType.InBattleDisplay);
		return addedCostValue >= finalCostValue;
	}

	public static bool SpellCastAble(GameStateInfo gameState, Item spellItem, Item targetItem, bool inBattleDisplay, out SpellCastFailureReason failureReason)
	{
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(gameState, inBattleDisplay)
		{
			spellItem = spellItem
		};
		if (targetItem != null)
		{
			displaySpellCastInfo.targetItem = targetItem;
		}
		if (!spellItem.IsManualSelectCost(gameState, GameEvent.TriggerType.InBattleDisplay))
		{
			displaySpellCastInfo.AddCost(displaySpellCastInfo.triggerType);
		}
		return displaySpellCastInfo.SpellCastAble(out failureReason);
	}

	public GameEvent GenerateSpellCastEvent(GameEvent.TriggerType triggerType)
	{
		return new GameEvent(GameEvent.Type.NodeCastSpell, triggerType)
		{
			gameState = gameState,
			spellInfo = this,
			SourceItem = spellItem,
			CasterItem = spellItem,
			TargetItem = targetItem,
			SelectedItems = precastSelectItems,
			negativeEnchantReplacementGenerator = negativeEnchantReplacementGenerator
		};
	}

	protected bool CheckSpellCastResolve(GameEvent.TriggerType triggerType)
	{
		return !gameState.gameEventManager.TriggerGenerateSpellCastResolveInfo(this, triggerType).reject;
	}

	public Enchant GetCastingOverrideSpellEnchant()
	{
		if (capturedFocusSpell != null)
		{
			if (capturedOverrideSpellEnchant?.def?.overrideSpell == capturedFocusSpell)
			{
				return capturedOverrideSpellEnchant;
			}
			return null;
		}
		if (forcedFocusSpell != null)
		{
			return null;
		}
		Enchant enchant = spellItem?.GetCurrentOverrideSpellEnchant();
		if (enchant?.def?.overrideSpell == focusSpell)
		{
			return enchant;
		}
		return null;
	}

	protected void CaptureFocusSpell()
	{
		capturedFocusSpell = forcedFocusSpell ?? spellItem?.CurrentSpell;
		capturedOverrideSpellEnchant = ((forcedFocusSpell != null) ? null : spellItem?.GetCurrentOverrideSpellEnchant());
		if (capturedOverrideSpellEnchant?.def?.overrideSpell != capturedFocusSpell)
		{
			capturedOverrideSpellEnchant = null;
		}
	}

	public bool IsFocusSpellCastTimingAble(GameEvent.TriggerType triggerType)
	{
		if (!hasFocusSpell || spellItem == null)
		{
			return false;
		}
		if (isTriggerCast)
		{
			return true;
		}
		if (GetCastingOverrideSpellEnchant() != null)
		{
			if (spellItem.OwnerBft != null)
			{
				return spellItem.OwnerBft.isPlayer;
			}
			return false;
		}
		return spellItem.IsCooldownCastAble(gameState, triggerType);
	}

	public static SpellCastInfo GetDefaultPlayerDisplayCastInfo(GameStateInfo gameState, Item item, GameEvent.TriggerType triggerType)
	{
		if (gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo.spellItem == item)
		{
			return gameState.UIManager.playerItemArea.controlInfo.displaySpellInfo;
		}
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(gameState);
		displaySpellCastInfo.spellItem = item;
		displaySpellCastInfo.SetDefaultTargetItem();
		displaySpellCastInfo.AddCost(triggerType);
		return displaySpellCastInfo;
	}
}
