public class CheckItemCastedThisTurn : CheckNode
{
	public GameEvent.EventItemType target;

	public bool excludeCurrentCast;

	public override bool? Check(GameEvent eve)
	{
		InGameRecorderManager inGameRecorderManager = eve.gameState?.recordManager;
		if (inGameRecorderManager == null)
		{
			return null;
		}
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return false;
		}
		if (excludeCurrentCast && eve.gameState.playerState.playerSpellInfo.castState != SpellCastInfo.State.Preparing)
		{
			for (int i = 0; i < inGameRecorderManager.castedItemsThisRound.Count - 1; i++)
			{
				if (inGameRecorderManager.castedItemsThisRound[i] == eventItem)
				{
					return true;
				}
			}
			return false;
		}
		return inGameRecorderManager.castedItemsThisRound.Contains(eventItem);
	}
}
