using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RealSpellCastInfo : SpellCastInfo
{
	public State castState;

	private bool spellCooldownApplied;

	private int appliedSpellCooldownDelta;

	private bool consumeCostWhenTriggered;

	private bool summonCompanion
	{
		get
		{
			Item item = spellItem;
			if (item != null && item.Companion)
			{
				ItemContainer owner = spellItem.owner;
				if (owner == null)
				{
					return true;
				}
				return owner.type != ItemContainer.ContainerType.Companion;
			}
			return false;
		}
	}

	public RealSpellCastInfo(GameStateInfo gameState)
	{
		base.gameState = gameState;
		castState = State.Preparing;
	}

	public static void TriggerSpellCast(GameStateInfo gameState, Item spellItem, Item targetItem, CasterType casterType, bool triggerCast = false, List<Item> preSelectedCosts = null, ItemSpell forcedSpell = null, Item triggerAnchorItem = null, bool suppressRemoveAfterCast = false, bool consumeCostWhenTriggered = false, bool triggerFromProtectedExile = false, List<Item> preSelectedItems = null, EnchantDefGenerator negativeEnchantReplacementGenerator = null, bool suppressChangeEnchantAndSpawnItemEffects = false, bool prepareTargetRequirementForTriggeredCast = false)
	{
		gameState.StartCoroutine(() => new RealSpellCastInfo(gameState).TriggerSpellCast(spellItem, targetItem, casterType, triggerCast, preSelectedCosts, forcedSpell, triggerAnchorItem, suppressRemoveAfterCast, consumeCostWhenTriggered, triggerFromProtectedExile, preSelectedItems, negativeEnchantReplacementGenerator, suppressChangeEnchantAndSpawnItemEffects, prepareTargetRequirementForTriggeredCast));
	}

	public IEnumerator TriggerSpellCast(Item spellItem, Item targetItem, CasterType casterType, bool triggerCast = false, List<Item> preSelectedCosts = null, ItemSpell forcedSpell = null, Item triggerAnchorItem = null, bool suppressRemoveAfterCast = false, bool consumeCostWhenTriggered = false, bool triggerFromProtectedExile = false, List<Item> preSelectedItems = null, EnchantDefGenerator negativeEnchantReplacementGenerator = null, bool suppressChangeEnchantAndSpawnItemEffects = false, bool prepareTargetRequirementForTriggeredCast = false)
	{
		isTriggerCast = triggerCast;
		continuousDamageActionTriggerCount = 0;
		this.consumeCostWhenTriggered = consumeCostWhenTriggered;
		base.spellItem = spellItem;
		forcedFocusSpell = forcedSpell;
		triggerCastAnchorItem = triggerAnchorItem;
		base.suppressRemoveAfterCast = suppressRemoveAfterCast;
		base.triggerFromProtectedExile = triggerFromProtectedExile;
		base.negativeEnchantReplacementGenerator = negativeEnchantReplacementGenerator;
		base.suppressChangeEnchantAndSpawnItemEffects = suppressChangeEnchantAndSpawnItemEffects;
		CaptureFocusSpell();
		base.casterType = casterType;
		if (preSelectedItems != null)
		{
			precastSelectItems.AddRange(preSelectedItems);
		}
		if (preSelectedCosts != null && preSelectedCosts.Count > 0)
		{
			Debug.Log($"[RealSpellCastInfo] TriggerSpellCast for {spellItem.ItemName} with {preSelectedCosts.Count} pre-selected costs.");
			foreach (Item preSelectedCost in preSelectedCosts)
			{
				if (preSelectedCost != null)
				{
					GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(preSelectedCost, this, GameEvent.TriggerType.Real);
					if (!resolveInfo.reject)
					{
						addedCostInfos.Add(new AddedCostInfo
						{
							item = preSelectedCost,
							costValue = 1 + resolveInfo.deltaValue
						});
					}
				}
			}
			realCostValue = base.requirement?.GetFinalCostValue(this, GameEvent.TriggerType.Real) ?? 0;
		}
		if (base.NeedSpellTargetItem)
		{
			if (targetItem != null)
			{
				base.targetItem = targetItem;
			}
			else
			{
				SetDefaultTargetItem();
			}
		}
		castState = State.PreCasting;
		if (casterType == CasterType.Player && spellItem.IsManualSelectCost(gameState, GameEvent.TriggerType.InBattleDisplay))
		{
			yield return ManualSelectCost();
		}
		if (((casterType == CasterType.Player) | prepareTargetRequirementForTriggeredCast) && base.requirement?.targetRequirement != null && preSelectedItems == null)
		{
			yield return PrepareTargetRequirement();
		}
		if (castState == State.PreCasting)
		{
			yield return OnCastSpell();
		}
	}

	public override void ReturnAllCost()
	{
		List<AddedCostInfo> list = new List<AddedCostInfo>(addedCostInfos);
		list.Reverse();
		foreach (AddedCostInfo item in list)
		{
			ReturnCost(item);
		}
	}

	private IEnumerator ManualSelectCost()
	{
		bool canceled = false;
		bool confirmed = false;
		int cost = base.requirement.GetFinalCostValue(this, GameEvent.TriggerType.InBattleDisplay);
		bool freelySelectCost = base.focusSpell?.ConsumesAllAvailableCost ?? false;
		DisplaySpellCastInfo displayCastInfo;
		if (cost != 0 || freelySelectCost)
		{
			displayCastInfo = new DisplaySpellCastInfo(this);
			gameState.actionManager.AcquireLock(ActionLock.Type.Default);
			SelectItemInfo.GenerateSelectItemsInfo(GenerateSpellCastEvent(GameEvent.TriggerType.InBattleDisplay), spellItem.owner.GetAllItems(), () => false, CanSelect, CanConfirm, OnConfirm, OnCancel, () => true, spellItem.OwnerBft, () => ScriptableObjectSingleton<TextDataPreset>.instance.term_select_item_as_cost, isSingleSelect: false, manualSelectedCosts, spellItem).TriggerSelect(gameState, PlayerSelectItemsEffect.SelectPanelType.Table, prepareSelect: true);
			yield return new WaitUntil(() => confirmed | canceled);
			if (canceled)
			{
				CleanSpellCast();
			}
		}
		bool CanConfirm()
		{
			displayCastInfo.ReturnAllCost();
			foreach (Item manualSelectedCost in manualSelectedCosts)
			{
				displayCastInfo.AddCostItem(manualSelectedCost, GameEvent.TriggerType.InBattleDisplay);
			}
			SpellCastFailureReason reason;
			if (freelySelectCost || HasEnoughCost())
			{
				return displayCastInfo.SpellCastAble(out reason);
			}
			return false;
		}
		bool CanSelect(Item item)
		{
			if (item == spellItem)
			{
				return false;
			}
			if (!freelySelectCost && HasEnoughCost())
			{
				return false;
			}
			bool num = base.hasFocusSpell && !manualSelectedCosts.Contains(item) && gameState.PlayerControlAble && IsFocusSpellCastTimingAble(GameEvent.TriggerType.InBattleDisplay) && !gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.ForbidAddCost);
			GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(item, this, GameEvent.TriggerType.InBattleDisplay);
			return num & !resolveInfo.reject;
		}
		bool HasEnoughCost()
		{
			int num = 0;
			for (int i = 0; i < manualSelectedCosts.Count; i++)
			{
				GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateAddCostResolveInfo(manualSelectedCosts[i], this, GameEvent.TriggerType.InBattleDisplay);
				num += 1 + resolveInfo.deltaValue;
			}
			return num >= cost;
		}
		void OnCancel()
		{
			canceled = true;
			gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
		void OnConfirm()
		{
			confirmed = true;
			gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		}
	}

	private IEnumerator PrepareTargetRequirement()
	{
		bool canceled = false;
		bool comfirmed = false;
		DisplaySpellCastInfo displaySpellCastInfo = new DisplaySpellCastInfo(this);
		displaySpellCastInfo.AddCost(GameEvent.TriggerType.InBattleDisplay);
		GameEvent eve = displaySpellCastInfo.GenerateSelectItemPreCastEvent();
		gameState.actionManager.AcquireLock(ActionLock.Type.Default);
		base.requirement.targetRequirement.TriggerPrepare(eve, delegate
		{
			comfirmed = true;
			targetItem = eve.TargetItem;
		}, delegate
		{
			canceled = true;
		});
		yield return new WaitUntil(() => comfirmed | canceled);
		gameState.actionManager.ReleaseLock(ActionLock.Type.Default);
		if (canceled)
		{
			CleanSpellCast();
		}
		else
		{
			precastSelectItems = eve.SelectedItems;
		}
	}

	private IEnumerator OnCastSpell()
	{
		AddCost(GameEvent.TriggerType.Real);
		GameEvent.ResolveInfo resolveInfo = gameState.gameEventManager.TriggerGenerateSpellCastResolveInfo(this, GameEvent.TriggerType.Real);
		gameState.gameEventManager.TriggerBeforeSpellCast(this);
		castState = State.Casting;
		List<Item> list = new List<Item>(base.addedCosts);
		BattleFieldTransform ownerBft = spellItem.OwnerBft;
		if (ownerBft != null && ownerBft.isPlayer)
		{
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_cast));
		}
		else
		{
			EnqueueAction(new PlayOneShotSoundAction(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_foe_card_cast));
		}
		GameEvent eve = GenerateSpellCastEvent(GameEvent.TriggerType.Real);
		if (spellItem != null && gameState.battleState.isInBattle && !isTriggerCast && !summonCompanion && GetCastingOverrideSpellEnchant() == null)
		{
			GameEvent.ResolveInfo resolveInfo2 = gameState.gameEventManager.TriggerSpellCastCoolDownResolve(this, GameEvent.TriggerType.Real);
			int coolDown = base.focusSpell.GetCoolDown(spellItem);
			coolDown += resolveInfo2.deltaValue;
			if (!resolveInfo2.reject)
			{
				spellCooldownApplied = true;
				appliedSpellCooldownDelta = coolDown;
				EnqueueAction(new ChangeCoolDownAction(spellItem, spellItem, coolDown));
			}
		}
		for (int i = 0; i < 1 + resolveInfo.deltaTimes; i++)
		{
			BattleFieldTransform casterBft = spellItem.OwnerBft;
			EnqueueAction(new ActionAction(delegate
			{
				if (!gameState.gameEventManager.TriggerSpellCastEffectResolve(this, GameEvent.TriggerType.Real).reject)
				{
					if (summonCompanion)
					{
						EnqueueAction(new SummonCompanionAction(spellItem, casterBft));
					}
					else
					{
						base.focusSpell.Cast(eve);
					}
				}
			}));
		}
		if (!isTriggerCast || consumeCostWhenTriggered)
		{
			EnqueueAction(new ActionAction(delegate
			{
				gameState.gameEventManager.TriggerBeforeSpellCastHandleCost(this);
			}));
			if (base.focusSpell.MovesHeadToCasterRightAsCost)
			{
				EnqueueAction(new MoveHeadToCasterRightCostAction(spellItem, this, base.focusSpell.costNode));
			}
			else
			{
				foreach (Item item in list)
				{
					EnqueueAction(new CostItemAction(item, this));
				}
			}
		}
		bool finished = false;
		EnqueueAction(new ActionAction(delegate
		{
			castState = State.FinishingCasting;
			gameState.gameEventManager.TriggerAfterSpellCast(this);
			if (spellItem != null && spellItem.ShouldAutoTransformAfterSpellCast(this))
			{
				EnqueueAction(new TransformAction(spellItem, spellItem));
				if (spellCooldownApplied && appliedSpellCooldownDelta != 0)
				{
					EnqueueAction(new ChangeCoolDownAction(spellItem, spellItem, -appliedSpellCooldownDelta));
				}
			}
			if (spellItem != null && !suppressRemoveAfterCast && spellItem.ConsumeRemoveAfterCast())
			{
				EnqueueAction(new ExileItemAction(spellItem, spellItem));
			}
			EnqueueAction(new ActionAction(delegate
			{
				CleanSpellCast();
				finished = true;
			}));
		}));
		yield return new WaitUntil(() => finished);
	}

	private void ReturnCost(AddedCostInfo info)
	{
		List<AddedCostInfo> list = addedCostInfos;
		if (list == null || !list.Contains(info))
		{
			return;
		}
		addedCostInfos.Remove(info);
		ItemContainer owner = info.item.owner;
		if (owner == null || owner.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		BattleFieldTransform ownerBft = info.item.OwnerBft;
		if (ownerBft == null || !ownerBft.isPlayer)
		{
			return;
		}
		List<Item> allItems = info.item.owner.GetAllItems();
		int? ownerSlotIndex = info.item.ownerSlotIndex;
		if (ownerSlotIndex.HasValue)
		{
			for (int i = ownerSlotIndex.Value + 1; i < allItems.Count; i++)
			{
				gameState.gameEventManager.TriggerSlotIndexChange(null, allItems[i], i - 1, 1);
			}
		}
	}

	private void EnqueueAction(GameAction action)
	{
		gameState.actionManager.EnqueueAction(action);
	}

	private void CleanSpellCast()
	{
		ReturnAllCost();
		ResetResourceDirectionReversal();
		spellItem = null;
		targetItem = null;
		forcedFocusSpell = null;
		triggerCastAnchorItem = null;
		suppressRemoveAfterCast = false;
		capturedFocusSpell = null;
		capturedOverrideSpellEnchant = null;
		spellCooldownApplied = false;
		appliedSpellCooldownDelta = 0;
		continuousDamageActionTriggerCount = null;
		gameState.UIManager.battlePanel.SetDirty();
		manualSelectedCosts.Clear();
		precastSelectItems.Clear();
		castState = State.Preparing;
		casterType = CasterType.None;
	}
}
