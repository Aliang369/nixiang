public class CheckPlayerHealthDamagedThisBattle : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.recordManager == null)
		{
			return null;
		}
		return eve.gameState.recordManager.playerHealthDamageThisBattle > 0;
	}
}
