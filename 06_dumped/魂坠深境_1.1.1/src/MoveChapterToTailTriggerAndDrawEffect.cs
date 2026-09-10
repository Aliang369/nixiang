using System.Collections.Generic;

public sealed class MoveChapterToTailTriggerAndDrawEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public ValueGenerator drawCountGenerator;

	public ValueGenerator triggerCountGenerator;

	public ValueGenerator upgradedTriggerCountGenerator;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = targetSelector?.GetSingleItem(eve);
		BattleFieldTransform eventBFT = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		if (item == null || eventBFT?.tableContainer == null)
		{
			return NodeCastResult.Failure;
		}
		Effect.EnqueueAction(eve, new MoveToSlotAction(eventBFT.tableContainer.ItemsCount - 1, item, eve.CasterItem));
		bool flag = upgradedCheck?.Check(eve) ?? eve.CasterItem?.upgraded ?? false;
		int num = ((flag && upgradedTriggerCountGenerator != null) ? upgradedTriggerCountGenerator : triggerCountGenerator)?.GenerateValue(eve) ?? ((!flag) ? 1 : 3);
		for (int i = 0; i < num; i++)
		{
			Effect.EnqueueAction(eve, new TriggerChapterAction(item));
		}
		int num2 = drawCountGenerator?.GenerateValue(eve) ?? 1;
		for (int j = 0; j < num2; j++)
		{
			Effect.EnqueueAction(eve, new DrawFromPileAction(eventBFT, eve.CasterItem));
		}
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, drawCountGenerator, triggerCountGenerator, upgradedTriggerCountGenerator, upgradedCheck };
	}
}
