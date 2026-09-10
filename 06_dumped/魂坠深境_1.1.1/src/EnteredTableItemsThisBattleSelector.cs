using System.Collections.Generic;

public class EnteredTableItemsThisBattleSelector : ItemTargetSelector
{
	public override List<Item> GetTargetItems(GameEvent eve)
	{
		InGameRecorderManager inGameRecorderManager = eve.gameState?.recordManager;
		if (inGameRecorderManager == null)
		{
			return null;
		}
		return new List<Item>(inGameRecorderManager.enteredTableItemsThisBattle);
	}
}
