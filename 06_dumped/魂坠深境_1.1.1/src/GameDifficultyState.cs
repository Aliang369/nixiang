using System.Collections.Generic;

public class GameDifficultyState : InGameManager
{
	[SaveField(null, null)]
	private new GameStateInfo gameState;

	[SaveField(null, null)]
	public List<PathLocation> reachedLocations = new List<PathLocation>();

	public DifficultyLevel contractDifficulty => gameState.contractState.selectedContract.difficultyLevel;

	public int reachedNodePointCount => reachedLocations.FindAll((PathLocation l) => l.isNodePoint).Count;

	public int reachedRoomsCount => reachedLocations.Count;

	public int CurrentStage => gameState.contractState.stage;

	private int EnemyLevelIncreasePerStage
	{
		get
		{
			int enemyLevelIncreasePerStage = ScriptableObjectSingleton<DungeonGenerationManager>.instance.EnemyLevelIncreasePerStage;
			return gameState.contractState.selectedContract.contractDef.contractRule?.GetEnemyLevelIncreasePerStage(gameState, enemyLevelIncreasePerStage) ?? enemyLevelIncreasePerStage;
		}
	}

	private int EnemyLevelOffset => gameState.contractState.selectedContract.contractDef.contractRule?.GetEnemyLevelOffset(gameState) ?? 0;

	public int StageBaseDifficulty => (gameState.contractState.stage - 1) * EnemyLevelIncreasePerStage + 1 + EnemyLevelOffset;

	public int StageTopDifficulty => gameState.contractState.stage * EnemyLevelIncreasePerStage + EnemyLevelOffset;

	public static int difficultyCount => DifficultyProgressCount(DifficultyLevel.NightmareD9);

	public override void Init(GameStateInfo gameState)
	{
		this.gameState = gameState;
	}

	public static int DifficultyProgressCount(DifficultyLevel level)
	{
		if (level < DifficultyLevel.Normal)
		{
			return 0;
		}
		return (int)(level - 1 + 1);
	}
}
