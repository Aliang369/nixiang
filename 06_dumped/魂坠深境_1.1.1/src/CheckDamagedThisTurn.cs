public class CheckDamagedThisTurn : CheckNode
{
	public GameEvent.EventItemType target;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.recordManager == null)
		{
			return null;
		}
		Item eventItem = eve.GetEventItem(target);
		if (eventItem == null)
		{
			return false;
		}
		return eve.gameState.recordManager.damagedItemsThisTurn.Contains(eventItem);
	}
}
