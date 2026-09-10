using System;
using System.Collections.Generic;
using System.Linq;
using GameRandom;

public sealed class EvilMagicTechniqueEffect : Effect
{
	public ValueGenerator drawCount;

	public ValueGenerator minimumDiscardCount;

	public EnchantDef flashbackEnchant;

	public ValueGenerator flashbackValue;

	public string panelHint;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform bft = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		int num = drawCount?.GenerateValue(eve) ?? 3;
		for (int i = 0; i < num; i++)
		{
			Effect.EnqueueAction(eve, new DrawFromPileAction(bft, eve.CasterItem));
		}
		Effect.EnqueueAction(eve, new ActionAction(delegate
		{
			OpenDiscardSelection(eve, bft);
		}));
		return NodeCastResult.Success;
	}

	private void OpenDiscardSelection(GameEvent eve, BattleFieldTransform bft)
	{
		List<Item> list = bft?.tableContainer?.GetAllItems();
		int minimum = minimumDiscardCount?.GenerateValue(eve) ?? 2;
		if (list == null || list.Count < minimum)
		{
			return;
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectAnyItem(list, delegate
		{
			int num = Math.Max(selected.Count - minimum, 0);
			List<Item> list2 = (from item2 in bft.tableContainer.GetAllItems()
				where item2 != eve.CasterItem && !selected.Contains(item2)
				select item2).ToList();
			for (int num2 = 0; num2 < num; num2++)
			{
				if (list2.Count <= 0)
				{
					break;
				}
				Item item = list2.SelectRandom(eve.gameState.randomManager.NextDouble);
				if (item != null && flashbackEnchant != null)
				{
					eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(item, eve.CasterItem, flashbackEnchant, flashbackValue?.GenerateValue(eve) ?? 1, popup: true, eve.spellInfo));
				}
			}
			foreach (Item item2 in selected.ToList())
			{
				eve.gameState.actionManager.EnqueueAction(new ConsumeItemAction(item2, ConsumeItemAction.ConsumeType.Effect));
			}
		}, null, selected, null, () => panelHint);
		selectItemInfo.canCancel = () => false;
		selectItemInfo.canConfirm = () => selected.Count >= minimum;
		selectItemInfo.selectorBft = bft;
		eve.gameState.actionManager.EnqueueAction(new SelectItemAction(selectItemInfo, PlayerSelectItemsEffect.SelectPanelType.Table));
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(drawCount?.GenerateValue(eve) ?? 3),
			baseValue = new NodeDisplayInfo.Element<int?>(minimumDiscardCount?.GenerateValue(eve) ?? 2),
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>((flashbackEnchant == null) ? null : new List<EnchantDef> { flashbackEnchant }),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>((flashbackEnchant == null) ? null : new List<Enchant>
			{
				new Enchant(flashbackEnchant, flashbackValue?.GenerateValue(eve) ?? 1)
			})
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawCount, minimumDiscardCount, flashbackValue };
	}
}
