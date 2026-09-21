public class CheckSpellCastThisTurn : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		InGameRecorderManager inGameRecorderManager = eve.gameState?.recordManager;
		if (inGameRecorderManager == null)
		{
			return null;
		}
		return inGameRecorderManager.castedItemsThisRound.Count == 0;
	}
}
