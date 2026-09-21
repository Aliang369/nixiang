using System.Collections.Generic;
using System.Linq;

public class DiscardSelectedChaptersForRepeatedTempHealthEffect : Effect
{
	public ItemTargetSelector targetSelector;

	public bool upgradedMultiplyByMaxChapter;

	public CheckNode upgradedCheck;

	protected override NodeCastResult OnCast(GameEvent eve)
	{
		List<Item> obj = targetSelector?.GetTargetItems(eve)?.ToList() ?? new List<Item>();
		bool flag = upgradedCheck?.Check(eve) ?? eve.CasterItem?.upgraded ?? false;
		int total = 0;
		foreach (Item item in obj)
		{
			if (item == null)
			{
				continue;
			}
			ProtectedInt value = item.counter;
			if (flag && upgradedMultiplyByMaxChapter)
			{
				value = (int)value * (int)item.maxCounter;
			}
			Effect.EnqueueAction(eve, new ConsumeItemAction(item, ConsumeItemAction.ConsumeType.Effect, null, null, delegate(Item discarded)
			{
				if (discarded != null)
				{
					total += (int)value;
				}
			}));
		}
		Effect.EnqueueAction(eve, new ActionAction(delegate
		{
			if (total > 0)
			{
				Effect.EnqueueAction(eve, new ChangeTempHealthAction(total, eve.GetEventBFT(GameEvent.EventBFTType.Owner)));
				eve.gameState.playerState?.AddPendingMusicianTempHealth(total);
			}
		}));
		return NodeCastResult.Success;
	}

	public override List<IEffectNode> GetChildren()
	{
		return new List<IEffectNode> { targetSelector, upgradedCheck };
	}
}
