using System.Collections.Generic;
using System.Linq;

public class DiscardChapterAndTriggerFinaleEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public ValueGenerator armorValueGenerator;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		Item item = targetSelector?.GetTargetItems(eve)?.FirstOrDefault();
		if (item == null)
		{
			return NodeCastResult.Failure;
		}
		Item caster = eve.CasterItem;
		ProtectedInt amount = item.maxCounter;
		int armor = armorValueGenerator?.GenerateValue(eve) ?? ((int)amount);
		Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect, null, null, delegate(Item discarded)
		{
			if (discarded == null)
			{
				return;
			}
			if (armor > 0)
			{
				Effect.EnqueueAction(eve, new ChangeCounterAction(-armor, caster, eve.CasterItem, withPopupText: false, consumeDefenceWhenDepleted: true));
			}
			GameEvent gameEvent = new GameEvent(GameEvent.Type.CounterReachMaxCounter, GameEvent.TriggerType.Real)
			{
				gameState = eve.gameState,
				TargetItem = discarded,
				CasterItem = discarded,
				SourceItem = caster,
				amount = amount
			};
			gameEvent.nodeTempValues["TriggerChapterIndexKey"] = amount;
			foreach (ChapterTriggerAbility item2 in discarded.GetDef()?.abilities?.OfType<ChapterTriggerAbility>() ?? Enumerable.Empty<ChapterTriggerAbility>())
			{
				if (item2.chapterTriggerType == ChapterTriggerAbility.ChapterTriggerType.FinalChapter)
				{
					item2.TriggerGameEvent(gameEvent);
				}
			}
		}));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, armorValueGenerator, upgradedCheck };
	}
}
