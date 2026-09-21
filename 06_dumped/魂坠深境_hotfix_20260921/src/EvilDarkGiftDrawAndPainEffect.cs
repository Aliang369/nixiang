using System;
using System.Collections.Generic;
using System.Linq;

public sealed class EvilDarkGiftDrawAndPainEffect : Effect
{
	public ValueGenerator drawCount;

	public EnchantDef painEnchant;

	public ValueGenerator painValue;

	public string panelHint;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform bft = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		List<Item> list = bft?.drawPileItemContainer?.GetAllItems();
		int count = Math.Min(drawCount?.GenerateValue(eve) ?? 3, list?.Count ?? 0);
		if (list == null || list.Count == 0)
		{
			return NodeCastResult.Success;
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectAnyItem(list, delegate
		{
			foreach (Item item in selected.ToList())
			{
				Effect.EnqueueAction(eve, new DrawSpecificItemFromPileAction(bft, item, eve.CasterItem, delegate(Item drawn)
				{
					if (drawn != null && !(painEnchant == null))
					{
						int delta = painValue?.GenerateValue(eve) ?? 3;
						eve.gameState.actionManager.EnqueueAction(new ChangeEnchantAction(drawn, eve.CasterItem, painEnchant, delta, popup: true, eve.spellInfo));
					}
				}));
			}
		}, null, selected, null, () => panelHint);
		selectItemInfo.canCancel = () => false;
		selectItemInfo.canConfirm = () => selected.Count <= count;
		selectItemInfo.canSelect = (Item item) => selected.Contains(item) || selected.Count < count;
		selectItemInfo.selectorBft = bft;
		Effect.EnqueueAction(eve, new SelectItemAction(selectItemInfo, PlayerSelectItemsEffect.SelectPanelType.PileSelect));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(drawCount?.GenerateValue(eve) ?? 3),
			refEnchantDefs = new NodeDisplayInfo.Element<List<EnchantDef>>((painEnchant == null) ? null : new List<EnchantDef> { painEnchant }),
			refEnchants = new NodeDisplayInfo.Element<List<Enchant>>((painEnchant == null) ? null : new List<Enchant>
			{
				new Enchant(painEnchant, painValue?.GenerateValue(eve) ?? 3)
			})
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { drawCount, painValue };
	}
}
