using System;
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
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		BattleFieldTransform eventBFT2 = eve.GetEventBFT(GameEvent.EventBFTType.Opponent);
		List<Item> pool = eventBFT?.tableContainer?.GetAllItems()?.Where((Item item) => item != eve.CasterItem).ToList();
		int num = friendlyCount?.GenerateValue(eve) ?? 3;
		if (pool == null || pool.Count < num)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list = eve.SelectedItems?.Where((Item item) => item != null).Distinct().ToList() ?? new List<Item>();
		if (eventBFT != null && eventBFT.isEnemy && list.Count == 0)
		{
			list = pool.SelectRandomCountUnique(num, eve.gameState.randomManager.NextDouble);
		}
		if (list.Count != num || list.Any((Item item) => !pool.Contains(item)))
		{
			return NodeCastResult.Failure;
		}
		foreach (Item item in list)
		{
			Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect));
		}
		List<Item> list2 = eventBFT2?.tableContainer?.GetAllItems();
		int count = Math.Min(enemyCount?.GenerateValue(eve) ?? 1, list2?.Count ?? 0);
		foreach (Item item2 in list2?.SelectRandomCountUnique(count, eve.gameState.randomManager.NextDouble) ?? new List<Item>())
		{
			Effect.EnqueueAction(eve, new ConsumeItemAction(item2, ConsumeItemAction.ConsumeType.Effect));
		}
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
