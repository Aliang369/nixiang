using System.Collections.Generic;
using System.Linq;
using GameRandom;

public class DrawRandomChapterLeftAndAdvanceEffect : Effect
{
	public ItemTagGenerator chapterTagGenerator;

	public ValueGenerator advanceCountGenerator;

	public ValueGenerator upgradedAdvanceCountGenerator;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		BattleFieldTransform bft = eve.GetEventBFT(GameEvent.EventBFTType.Owner);
		ItemTag tag = chapterTagGenerator?.GenerateSingleItemTag(eve);
		bool flag = upgradedCheck?.Check(eve) ?? eve.CasterItem?.upgraded ?? false;
		int count = ((flag && upgradedAdvanceCountGenerator != null) ? upgradedAdvanceCountGenerator : advanceCountGenerator)?.GenerateValue(eve) ?? ((!flag) ? 1 : 2);
		List<Item> list = bft?.drawPileItemContainer?.GetAllItems().Where((Item i) => i?.HasTag(tag) ?? false).ToList();
		if (list == null || list.Count == 0 || count <= 0)
		{
			return NodeCastResult.Success;
		}
		Item mustDrawItem = list.SelectRandom(eve.gameState.randomManager.NextDouble);
		int valueOrDefault = (eve.CasterItem?.ownerSlotIndex).GetValueOrDefault();
		Effect.EnqueueAction(eve, new DrawSpecificItemFromPileAction(bft, mustDrawItem, eve.CasterItem, delegate(Item drawn)
		{
			if (drawn != null)
			{
				Effect.EnqueueAction(eve, new AdvanceGeneratedChapterAction(drawn, eve.CasterItem, bft.tableContainer, tag, count));
			}
		}, valueOrDefault));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { chapterTagGenerator, advanceCountGenerator, upgradedAdvanceCountGenerator, upgradedCheck };
	}
}
