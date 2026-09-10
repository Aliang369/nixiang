using System.Collections.Generic;
using System.Linq;
using GameRandom;

public sealed class EvilPresentTrickEffect : Effect
{
	public ValueGenerator friendlyCount;

	public ValueGenerator enemyCount;

	public string panelHint;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item caster = eve.CasterItem;
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		BattleFieldTransform opponent = eve.GetEventBFT(GameEvent.EventBFTType.Opponent);
		List<Item> list = eventBFT?.tableContainer?.GetAllItems()?.Where((Item item) => item != caster).ToList();
		int num = friendlyCount?.GenerateValue(eve) ?? 3;
		if (list == null || list.Count < num)
		{
			return NodeCastResult.Failure;
		}
		List<Item> selected = new List<Item>();
		SelectItemInfo selectItemInfo = SelectItemInfo.SimpleSelectCountItem(list, delegate
		{
			foreach (Item item in selected.ToList())
			{
				Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect));
			}
			List<Item> obj = opponent?.tableContainer?.GetAllItems();
			foreach (Item item2 in RandomExtension.SelectRandomCountUnique(count: enemyCount?.GenerateValue(eve) ?? 1, pool: obj?, nextRandomGetter: eve.gameState.randomManager.NextDouble) ?? new List<Item>())
			{
				Effect.EnqueueAction(eve, new ConsumeItemAction(item2, ConsumeItemAction.ConsumeType.Effect));
			}
		}, null, selected, num, null, () => panelHint);
		selectItemInfo.canCancel = () => false;
		selectItemInfo.selectorBft = eventBFT;
		Effect.EnqueueAction(eve, new SelectItemAction(selectItemInfo, PlayerSelectItemsEffect.SelectPanelType.Table));
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(friendlyCount?.GenerateValue(eve) ?? 3),
			baseValue = new NodeDisplayInfo.Element<int?>(enemyCount?.GenerateValue(eve) ?? 1)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { friendlyCount, enemyCount };
	}
}
