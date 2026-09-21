using System;

public class ConsumeItemAction : ExecuteAction
{
	public enum ConsumeType
	{
		Effect,
		SpellCost,
		Overflow,
		Damage,
		Deathblow
	}

	private const int DetailedSpellCostDiscardNfxCount = 4;

	private const int FastSpellCostDiscardNfxCount = 1;

	private readonly bool burnCostSpellCost;

	private readonly SpellCastInfo castInfo;

	private readonly HitNFXConfig hitEffect;

	private readonly Item targetItem;

	private readonly ConsumeType type;

	private readonly Action<Item> onItemDiscarded;

	public ConsumeItemAction(Item targetItem, ConsumeType type, SpellCastInfo castInfo = null, HitNFXConfig hitEffect = null, Action<Item> onItemDiscarded = null)
	{
		this.type = type;
		this.targetItem = targetItem;
		this.castInfo = castInfo;
		this.hitEffect = hitEffect;
		this.onItemDiscarded = onItemDiscarded;
		burnCostSpellCost = type == ConsumeType.SpellCost && CostItemAction.IsBurnCostCast(castInfo);
		Ignore(ActionLock.Type.RemoveItem);
	}

	protected override void Update()
	{
		if (targetItem == null || targetItem.owner == null || targetItem.owner.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		Item item = castInfo?.spellItem;
		if (targetItem.BlocksExternalAction(item))
		{
			return;
		}
		if (burnCostSpellCost)
		{
			EnqueueAction(new ExileItemAction(targetItem, castInfo.spellItem, hitEffect));
		}
		else if (!gameState.gameEventManager.TriggerResolveConsumeInfo(targetItem, item, type, castInfo, GameEvent.TriggerType.Real).reject)
		{
			ItemContainer container = targetItem.owner;
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerBeforeConsume(targetItem, castInfo, type);
			});
			EnqueueAction(DiscardItem);
			EnqueueAction(delegate
			{
				gameState.gameEventManager.TriggerAfterConsume(targetItem, castInfo, type, container);
			});
		}
	}

	private void DiscardItem()
	{
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		Item item = targetItem;
		if (item == null || item.owner?.type != ItemContainer.ContainerType.Table)
		{
			return;
		}
		ItemContainer wastePile = targetItem.OwnerBft?.wastePileItemContainer;
		if (wastePile == null)
		{
			return;
		}
		if (ShouldPlayDiscardNfx())
		{
			EnqueueAction(new DisplayNFXAction(ScriptableObjectSingleton<VFXDataPreset>.instance.effect_discard, targetItem));
		}
		hitEffect?.Play(gameState, targetItem);
		EnqueueAction(new TransferItemAction(targetItem, wastePile, targetItem, TransferItemAction.TransferAnimType.None, null, delegate(Item discardedItem)
		{
			if (discardedItem?.owner == wastePile)
			{
				onItemDiscarded?.Invoke(discardedItem);
			}
		}));
		if (!gameState.gameEventManager.TriggerConsumeResetCounterResolve(targetItem).reject)
		{
			EnqueueAction(new ResetCounterAction(targetItem, targetItem));
		}
		EnqueueAction(new ActionAction(delegate
		{
			if (targetItem.OwnerBft.isPlayer)
			{
				gameState.UIManager.playerItemArea.OnRemoveItem(targetItem);
			}
			if (targetItem.OwnerBft.isEnemy)
			{
				gameState.UIManager.enemyBftUIDisplay.tableItemArea.OnRemoveItem(targetItem);
			}
		}));
		if (targetItem.OwnerBft != null)
		{
			Bootstrap.instance.audioManager.PlayOneShot(ScriptableObjectSingleton<AudioData>.instance.sfx_gameplay_card_discard);
		}
	}

	private bool ShouldPlayDiscardNfx()
	{
		if (type != ConsumeType.SpellCost || castInfo?.addedCostInfos == null)
		{
			return true;
		}
		int num = ((gameState.applicationState.settingManager.battleTimeScaleTypeSetting == TimeScaleManager.TimeScaleType.X4) ? 1 : 4);
		for (int i = 0; i < castInfo.addedCostInfos.Count; i++)
		{
			if (castInfo.addedCostInfos[i].item == targetItem)
			{
				return i < num;
			}
		}
		return true;
	}
}
