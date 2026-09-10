public class TurnCountGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.gameState?.recordManager == null)
		{
			return null;
		}
		return eve.gameState.battleState.turnCount;
	}
}
