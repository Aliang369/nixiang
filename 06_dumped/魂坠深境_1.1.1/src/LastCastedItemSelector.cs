using System.Collections.Generic;

public class LastCastedItemSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		InGameRecorderManager inGameRecorderManager = eve.gameState?.recordManager;
		if (inGameRecorderManager == null)
		{
			return null;
		}
		if (inGameRecorderManager.castedItemsThisRound == null || eve.gameState.recordManager.castedItemsThisRound.Count == 0)
		{
			return new List<Item>();
		}
		return new List<Item> { inGameRecorderManager.castedItemsThisRound.FindLast((Item i) => i.owner != null) };
	}
}
