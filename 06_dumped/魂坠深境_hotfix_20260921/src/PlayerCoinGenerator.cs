public class PlayerCoinGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		PlayerGameStateInfo playerGameStateInfo = eve.gameState?.playerState;
		if (playerGameStateInfo == null)
		{
			return null;
		}
		return playerGameStateInfo.coinCount;
	}
}
