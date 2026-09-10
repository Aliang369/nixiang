public class PlayerCrystalGenerator : ValueGenerator
{
	public override int? GenerateValue(GameEvent eve)
	{
		if (eve.triggerType == GameEvent.TriggerType.OutBattleDisplay)
		{
			return null;
		}
		PlayerGameStateInfo playerGameStateInfo = eve.gameState?.playerState;
		if (playerGameStateInfo == null)
		{
			return null;
		}
		return playerGameStateInfo.crystalCount;
	}
}
