public class CheckCurrentBattleIsEliteBattle : CheckNode
{
	public override bool? Check(GameEvent eve)
	{
		GameStateInfo gameState = eve.gameState;
		bool? flag = gameState != null && gameState.currentEncounter?.setupInfo?.enemySpawnType == EnemyEncounterType.Elite;
		return flag ?? ((bool?)null);
	}
}
