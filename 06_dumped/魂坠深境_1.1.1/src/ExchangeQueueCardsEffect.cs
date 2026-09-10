using System.Collections.Generic;
using UnityEngine;

public sealed class ExchangeQueueCardsEffect : Effect
{
	public ValueGenerator opponentDrawReductionGenerator;

	private static int GetOpponentDrawCount(int opponentSnapshotCount, int reduction)
	{
		return Mathf.Max(0, opponentSnapshotCount - reduction);
	}

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform battleFieldTransform = eve.CasterSkill?.OwnerBft;
		BattleFieldTransform battleFieldTransform2 = eve.gameState?.battleState?.GetOpponent(battleFieldTransform);
		ItemContainer itemContainer = battleFieldTransform?.tableContainer;
		ItemContainer itemContainer2 = battleFieldTransform2?.tableContainer;
		if (itemContainer == null || itemContainer2 == null)
		{
			return NodeCastResult.Failure;
		}
		List<Item> list = new List<Item>(itemContainer.GetAllItems());
		List<Item> list2 = new List<Item>(itemContainer2.GetAllItems());
		int valueOrDefault = (opponentDrawReductionGenerator?.GenerateValue(eve)).GetValueOrDefault();
		int opponentDrawCount = GetOpponentDrawCount(list2.Count, valueOrDefault);
		foreach (Item item in list)
		{
			Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect));
		}
		foreach (Item item2 in list2)
		{
			Effect.EnqueueAction(eve, new ConsumeItemAction(item2, ConsumeItemAction.ConsumeType.Effect));
		}
		for (int i = 0; i < list.Count; i++)
		{
			Effect.EnqueueAction(eve, new DrawFromPileAction(battleFieldTransform, null));
		}
		for (int j = 0; j < opponentDrawCount; j++)
		{
			Effect.EnqueueAction(eve, new DrawFromPileAction(battleFieldTransform2, null));
		}
		return NodeCastResult.Success;
	}

	public override NodeDisplayInfo GetNodeDisplayInfo(GameEvent eve)
	{
		int valueOrDefault = (opponentDrawReductionGenerator?.GenerateValue(eve)).GetValueOrDefault();
		return new NodeDisplayInfo
		{
			value = new NodeDisplayInfo.Element<int?>(valueOrDefault),
			outputBool = new NodeDisplayInfo.Element<bool?>(valueOrDefault > 0)
		};
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { opponentDrawReductionGenerator };
	}
}
