public class CheckEnemyHealth : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.battleState?.enemy?.bft == null)
		{
			return null;
		}
		return (int)eve.gameState.battleState.enemy.bft.health <= 0;
	}
}
