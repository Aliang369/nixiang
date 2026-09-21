public class PlayerProfileFinishedGameCount : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		return eve.gameState.applicationState.gameStatisticsManager.gameCount;
	}
}
