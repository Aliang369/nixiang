using System.Collections.Generic;
using System.Linq;

public class SpiderWebDiscardSpreadAbility : Ability
{
	private const string PendingWebDiscardKey = "SpiderWebDiscardSpreadAbility.PendingWebDiscard";

	private const string AdjacentItemsSnapshotKey = "SpiderWebDiscardSpreadAbility.AdjacentItemsSnapshot";

	private const string SpellCostAdjacentItemsSnapshotKey = "SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot";

	public override IEnumerable<GameEvent.Type> GetAllSubscribeGameEventType()
	{
		yield return GameEvent.Type.BeforeSpellCast;
		yield return GameEvent.Type.BeforeConsume;
		yield return GameEvent.Type.AfterSpellCast;
	}

	public override bool CheckTriggerGameEventAble(GameEvent eve)
	{
		if (eve != null && eve.type == GameEvent.Type.BeforeSpellCast)
		{
			if (base.CheckTriggerGameEventAble(eve) && eve.CasterEnchant?.attachItem != null && (int)eve.CasterEnchant.value > 0)
			{
				SpellCastInfo spellInfo = eve.spellInfo;
				if (spellInfo != null && spellInfo.addedCosts?.Contains(eve.CasterEnchant.attachItem) == true)
				{
					ItemContainer owner = eve.CasterEnchant.attachItem.owner;
					if (owner == null)
					{
						return false;
					}
					return owner.type == ItemContainer.ContainerType.Table;
				}
			}
			return false;
		}
		if (eve != null && eve.type == GameEvent.Type.AfterSpellCast)
		{
			if (base.CheckTriggerGameEventAble(eve) && eve.CasterEnchant?.attachItem != null)
			{
				return eve.CasterEnchant.attachItem.tempValues.ContainsKey("SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot");
			}
			return false;
		}
		if (base.CheckTriggerGameEventAble(eve) && eve.CasterEnchant?.attachItem != null && eve.TargetItem == eve.CasterEnchant.attachItem && (int)eve.CasterEnchant.value > 0)
		{
			Item targetItem = eve.TargetItem;
			if (targetItem == null)
			{
				return false;
			}
			return targetItem.owner?.type == ItemContainer.ContainerType.Table;
		}
		return false;
	}

	public override void TriggerGameEvent(GameEvent eve)
	{
		if (!CheckTriggerGameEventAble(eve))
		{
			return;
		}
		if (eve.type == GameEvent.Type.BeforeSpellCast)
		{
			eve.CasterEnchant.attachItem.tempValues["SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot"] = SpiderContractEnemyAbilityUtility.GetAdjacentItems(eve.CasterEnchant.attachItem).ToList();
			return;
		}
		if (eve.type == GameEvent.Type.AfterSpellCast)
		{
			eve.CasterEnchant.attachItem.tempValues.Remove("SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot");
			return;
		}
		List<Item> list = ((eve.consumeType == ConsumeItemAction.ConsumeType.SpellCost) ? GetSpellCostAdjacentItems(eve.TargetItem) : GetAdjacentItems(eve.TargetItem)).Where((Item item) => item != null && item.owner?.type == ItemContainer.ContainerType.Table).ToList();
		if (eve.consumeType == ConsumeItemAction.ConsumeType.SpellCost)
		{
			eve.TargetItem.tempValues.Remove("SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot");
		}
		foreach (Item item in list)
		{
			EnqueueWebDiscard(eve.gameState, item);
		}
		eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(eve.TargetItem, eve.SourceItem, eve.CasterEnchant.def, -1));
		EnqueueWebStackPopup(eve.gameState, eve.TargetItem, eve.CasterEnchant.def);
	}

	private static void EnqueueWebDiscard(GameStateInfo gameState, Item item)
	{
		if (item == null || item.tempValues.ContainsKey("SpiderWebDiscardSpreadAbility.PendingWebDiscard"))
		{
			return;
		}
		item.tempValues["SpiderWebDiscardSpreadAbility.PendingWebDiscard"] = true;
		item.tempValues["SpiderWebDiscardSpreadAbility.AdjacentItemsSnapshot"] = SpiderContractEnemyAbilityUtility.GetAdjacentItems(item).ToList();
		gameState.actionManager.EnqueueAction(new ActionAction(delegate
		{
			ItemContainer owner = item.owner;
			if (owner == null || owner.type != ItemContainer.ContainerType.Table || item.BlocksExternalAction(null))
			{
				item.tempValues.Remove("SpiderWebDiscardSpreadAbility.PendingWebDiscard");
				item.tempValues.Remove("SpiderWebDiscardSpreadAbility.AdjacentItemsSnapshot");
			}
			else
			{
				gameState.actionManager.EnqueueAction(new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect));
				gameState.actionManager.EnqueueAction(new ActionAction(delegate
				{
					item.tempValues.Remove("SpiderWebDiscardSpreadAbility.PendingWebDiscard");
					item.tempValues.Remove("SpiderWebDiscardSpreadAbility.AdjacentItemsSnapshot");
				}));
			}
		}));
	}

	private static IEnumerable<Item> GetAdjacentItems(Item item)
	{
		if (item != null && item.tempValues.TryGetValue("SpiderWebDiscardSpreadAbility.AdjacentItemsSnapshot", out var value) && value is List<Item> result)
		{
			return result;
		}
		return SpiderContractEnemyAbilityUtility.GetAdjacentItems(item);
	}

	private static IEnumerable<Item> GetSpellCostAdjacentItems(Item item)
	{
		if (item != null && item.tempValues.TryGetValue("SpiderWebDiscardSpreadAbility.SpellCostAdjacentItemsSnapshot", out var value) && value is List<Item> result)
		{
			return result;
		}
		return GetAdjacentItems(item);
	}

	private static void EnqueueWebStackPopup(GameStateInfo gameState, Item item, EnchantDef enchantDef)
	{
		if (item != null && !(enchantDef == null) && !item.IsHidden(gameState))
		{
			PopupTextAction action = new PopupTextAction(item, (enchantDef.Name + "-1").ColorText(enchantDef.displayColor), ScriptableObjectSingleton<VFXDataPreset>.instance.enchantPopupTextPrefab)
			{
				popupIcon = enchantDef.Icon
			};
			gameState.actionManager.EnqueueAction(action);
		}
	}
}
