public class CheckGameEndSuccess : CheckNode
{
	public bool allowNoFullGame;

	public override bool? Check(GameEvent eve)
	{
		if (eve.type != GameEvent.Type.AfterGameEnd && eve.type != GameEvent.Type.BeforeGameEnd)
		{
			return null;
		}
		if (allowNoFullGame)
		{
			return eve.succesGame;
		}
		return eve.succesGame && eve.gameState.contractState.stage >= 6;
	}
}
