public class DisplaySpellCastInfo : SpellCastInfo
{
	private readonly bool inBattleDisplay;

	public GameEvent.TriggerType triggerType
	{
		get
		{
			if (!inBattleDisplay)
			{
				return GameEvent.TriggerType.OutBattleDisplay;
			}
			return GameEvent.TriggerType.InBattleDisplay;
		}
	}

	private bool companionCast
	{
		get
		{
			Item item = spellItem;
			if (item != null && item.Companion)
			{
				return spellItem.owner.type == ItemContainer.ContainerType.Companion;
			}
			return false;
		}
	}

	private bool isEnemyCast
	{
		get
		{
			if (spellItem.OwnerBft != null)
			{
				return spellItem.OwnerBft.isEnemy;
			}
			return false;
		}
	}

	public DisplaySpellCastInfo(GameStateInfo gameState, bool inBattleDisplay = false)
	{
		base.gameState = gameState;
		this.inBattleDisplay = inBattleDisplay;
	}

	public DisplaySpellCastInfo(RealSpellCastInfo realSpellCastInfo)
	{
		gameState = realSpellCastInfo.gameState;
		inBattleDisplay = true;
		spellItem = realSpellCastInfo.spellItem;
		targetItem = realSpellCastInfo.targetItem;
		forcedFocusSpell = realSpellCastInfo.forcedFocusSpell;
		capturedFocusSpell = realSpellCastInfo.focusSpell;
		capturedOverrideSpellEnchant = realSpellCastInfo.GetCastingOverrideSpellEnchant();
		isTriggerCast = realSpellCastInfo.isTriggerCast;
		casterType = realSpellCastInfo.casterType;
		resourceDirectionReversalActive = realSpellCastInfo.resourceDirectionReversalActive;
		resourceDirectionReversalApplied = realSpellCastInfo.resourceDirectionReversalApplied;
		resourceDirectionReversalResolved = true;
	}

	public void CancelSpell()
	{
		addedCostInfos.Clear();
		spellItem = null;
		targetItem = null;
		forcedFocusSpell = null;
		capturedFocusSpell = null;
		capturedOverrideSpellEnchant = null;
		isTriggerCast = false;
		allowAddedCostAsTarget = false;
		casterType = CasterType.None;
		ResetResourceDirectionReversal();
	}

	public static bool CheckSpellCastResolve(GameStateInfo gameState, Item spellItem, bool inBattleDisplay)
	{
		DisplaySpellCastInfo obj = new DisplaySpellCastInfo(gameState, inBattleDisplay)
		{
			spellItem = spellItem
		};
		obj.AddCost(obj.triggerType);
		return obj.CheckSpellCastResolve(obj.triggerType);
	}

	public bool SpellCastAble(out SpellCastFailureReason reason)
	{
		if (!base.hasFocusSpell)
		{
			reason = SpellCastFailureReason.NoFocusSpell;
			return false;
		}
		if (!gameState.PlayerControlAble)
		{
			reason = SpellCastFailureReason.NotControl;
			return false;
		}
		if (base.focusSpell.MovesHeadToCasterRightAsCost)
		{
			ItemContainer owner = spellItem.owner;
			if (owner == null || owner.type != ItemContainer.ContainerType.Table)
			{
				reason = SpellCastFailureReason.ConditionFailure;
				return false;
			}
			if (spellItem.owner.firstItem == spellItem)
			{
				reason = SpellCastFailureReason.AlreadyAtHead;
				return false;
			}
			if (spellItem.owner.GetRawIndex(spellItem) == 1 && spellItem.owner.firstItem?.GetDef() == spellItem.GetDef())
			{
				reason = SpellCastFailureReason.ConsecutiveDazzlingAtHead;
				return false;
			}
		}
		bool flag = spellItem.owner != null;
		bool flag2 = IsFocusSpellCastTimingAble(triggerType);
		if (!flag || !(spellItem.OwnerBft.isEnemy | flag2))
		{
			if (flag && !spellItem.OwnerBft.isEnemy && !flag2)
			{
				reason = SpellCastFailureReason.NotCoolDowned;
				return false;
			}
			reason = SpellCastFailureReason.ConditionFailure;
			return false;
		}
		if (base.requirement != null && !base.requirement.CheckCastCheckNodeAccept(this))
		{
			reason = SpellCastFailureReason.ConditionFailure;
			return false;
		}
		if (!isEnemyCast && !companionCast && !CheckCostFullFilled(triggerType) && !spellItem.IsManualSelectCost(gameState, triggerType) && !CanCastWithoutCostFulfill())
		{
			reason = SpellCastFailureReason.NotEnoughCost;
			return false;
		}
		bool num = !base.NeedSpellTargetItem || CheckItemAsTargetAble(targetItem);
		bool flag3 = CheckSpellCastResolve(triggerType);
		if (!(num & flag3))
		{
			reason = SpellCastFailureReason.ConditionFailure;
			return false;
		}
		reason = SpellCastFailureReason.None;
		return true;
	}

	private bool CheckCostFullFilled(GameEvent.TriggerType triggerType)
	{
		if (base.requirement == null)
		{
			return true;
		}
		ItemSpell itemSpell = base.focusSpell;
		if (itemSpell != null && itemSpell.MovesHeadToCasterRightAsCost)
		{
			return true;
		}
		return base.addedCostValue >= base.requirement.GetFinalCostValue(this, triggerType);
	}

	private bool CanCastWithoutCostFulfill()
	{
		ItemSpell itemSpell = base.focusSpell;
		if (itemSpell != null && itemSpell.ConsumesAllAvailableCost)
		{
			return true;
		}
		ItemSpell itemSpell2 = base.focusSpell;
		if (itemSpell2 != null && itemSpell2.allowCastWithoutEnoughCost)
		{
			return true;
		}
		if (gameState == null)
		{
			return false;
		}
		return gameState.gameEventManager.TriggerCanCastWithoutFulfillCostResolve(this).promote;
	}

	public GameEvent GenerateSelectItemPreCastEvent()
	{
		return new GameEvent(GameEvent.Type.TargetRequirement, GameEvent.TriggerType.Real)
		{
			gameState = gameState,
			CasterItem = spellItem,
			spellInfo = this
		};
	}
}
