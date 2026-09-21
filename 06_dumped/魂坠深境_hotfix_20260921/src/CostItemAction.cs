public class CostItemAction : ExecuteAction
{
	private const string BurnCostKeyword = "<BurnCost>";

	private readonly bool burnCost;

	private readonly SpellCastInfo castInfo;

	private readonly Item targetItem;

	public CostItemAction(Item targetItem, SpellCastInfo castInfo)
	{
		this.targetItem = targetItem;
		this.castInfo = castInfo;
		burnCost = IsBurnCostCast(castInfo);
		Ignore(ActionLock.Type.RemoveItem);
	}

	public static bool IsBurnCostCast(SpellCastInfo castInfo)
	{
		if (castInfo != null && castInfo.focusSpell?.burnCost == true)
		{
			return true;
		}
		Item item = castInfo?.spellItem;
		if (item == null)
		{
			return false;
		}
		ItemDefinition def = item.def;
		if ((object)def != null && def.burnCost)
		{
			return true;
		}
		ItemSpell currentSpell = item.CurrentSpell;
		if (currentSpell == null || !currentSpell.burnCost)
		{
			ItemSpell baseSpell = item.BaseSpell;
			if (baseSpell == null || !baseSpell.burnCost)
			{
				ItemSpell originalSpell = item.GetOriginalSpell();
				if (originalSpell == null || !originalSpell.burnCost)
				{
					ItemDefinition.ItemSkinConfig currentItemSkinConfig = item.CurrentItemSkinConfig;
					if (currentItemSkinConfig == null || currentItemSkinConfig.overrideSpell?.burnCost != true)
					{
						string currentSpellFormatDescription = item.CurrentSpellFormatDescription;
						if (currentSpellFormatDescription == null || !currentSpellFormatDescription.Contains("<BurnCost>"))
						{
							ItemDefinition def2 = item.def;
							if ((object)def2 == null)
							{
								return false;
							}
							return def2.spellFormtDescription?.Contains("<BurnCost>") == true;
						}
						return true;
					}
				}
			}
		}
		return true;
	}

	protected override void Update()
	{
		if (targetItem == null)
		{
			return;
		}
		gameState.gameEventManager.TriggerBeforeCost(targetItem, castInfo);
		ItemContainer owner = targetItem.owner;
		if (owner == null || !owner.IsBattleContainer())
		{
			EnqueueAction(new ActionAction(delegate
			{
				gameState.gameEventManager.TriggerAfterCost(targetItem, castInfo);
			}));
			return;
		}
		if (burnCost)
		{
			EnqueueAction(new ExileItemAction(targetItem, castInfo.spellItem));
			EnqueueAction(new ActionAction(delegate
			{
				gameState.gameEventManager.TriggerAfterCost(targetItem, castInfo);
			}));
			return;
		}
		if (!gameState.gameEventManager.TriggerResolveSpellCostConsumeInfo(targetItem, castInfo.spellItem, GameEvent.TriggerType.Real).reject)
		{
			EnqueueAction(new ConsumeItemAction(targetItem, ConsumeItemAction.ConsumeType.SpellCost, castInfo));
		}
		EnqueueAction(new ActionAction(delegate
		{
			gameState.gameEventManager.TriggerAfterCost(targetItem, castInfo);
		}));
	}
}
