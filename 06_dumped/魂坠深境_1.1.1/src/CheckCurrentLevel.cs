public class CheckCurrentLevel : CheckNode
{
	public Level level;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.contractState?.currentLevel == level)
		{
			return true;
		}
		return false;
	}
}
