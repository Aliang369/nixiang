public class CheckInBattle : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		return eve.gameState.battleState != null && eve.gameState.battleState.isInBattle;
	}
}
