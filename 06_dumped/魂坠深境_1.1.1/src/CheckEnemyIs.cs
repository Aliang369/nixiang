using System.Collections.Generic;

public class CheckEnemyIs : CheckNode
{
	public EnemyAssetRef enemy;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState?.battleState?.enemy == null)
		{
			return null;
		}
		if (eve.gameState.battleState.enemy.component.assetIndex == enemy.GetAssetPreData(eve.gameState.applicationState.assetCatalog).index)
		{
			return true;
		}
		return false;
	}

	public List<string> GetAllEnemyIds()
	{
		return new List<string>();
	}
}
