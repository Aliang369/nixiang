using System.Collections.Generic;

public class EnemyInitInfo
{
	private GameStateInfo gameState;

	public EnemyInfoData enemyInfoData;

	public CharacterAIComponent.EnemySpawnSetup sourceDifficultySetup;

	public EnemyEncounterType encounterType;

	public float enemyProgressLevel;

	public DifficultyLevel difficulty;

	public bool exIntent;

	private EnemyInitInfo()
	{
	}

	public EnemyInitInfo(GameStateInfo gameState, EnemyEncounterType encounterType, CharacterAIComponent.EnemySpawnSetup sourceDifficultySetup, EnemyInfoData enemyInfoData = null)
	{
		this.gameState = gameState;
		this.enemyInfoData = enemyInfoData;
		this.encounterType = encounterType;
		this.sourceDifficultySetup = sourceDifficultySetup;
		enemyProgressLevel = gameState.currentEncounter.enemyLevel;
		if ((encounterType == EnemyEncounterType.Normal && gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DeadlierNormalEnemy)) | ((encounterType == EnemyEncounterType.Elite || encounterType == EnemyEncounterType.Boss) && gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.DeadlierEliteEnemy)))
		{
			enemyProgressLevel++;
		}
		difficulty = gameState.difficultyState.contractDifficulty;
		exIntent = false;
		if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.AllEnemyIntentExtend))
		{
			exIntent = true;
		}
		else if (gameState.globalVariableStateInfo.GetVariableFlag(InGameGlobalVariableStateInfo.HalfEnemyIntentExtend))
		{
			exIntent = gameState.randomManager.NextFloat(0f, 1f) > 0.5f;
		}
	}

	public EnemyInitInfo(CharacterAIComponent.EnemySpawnSetup sourceDifficultySetup, EnemyEncounterType encounterType, float enemyProgressLevel, DifficultyLevel difficulty, bool exIntent)
	{
		this.sourceDifficultySetup = sourceDifficultySetup;
		this.encounterType = encounterType;
		this.enemyProgressLevel = enemyProgressLevel;
		this.difficulty = difficulty;
		this.exIntent = exIntent;
	}

	private void ApplyContractInitialStrength(Item item)
	{
		if (item != null && gameState != null && enemyInfoData != null)
		{
			gameState.contractState?.selectedContract?.contractDef?.ApplyEnemyGeneratedItemInitialState(gameState, enemyInfoData, encounterType, item);
		}
	}

	public List<Item> GenerateInitDeckItems()
	{
		List<Item> list = new List<Item>();
		foreach (ItemGeneration itemGeneration in sourceDifficultySetup.itemGenerations)
		{
			if (itemGeneration?.item == null)
			{
				continue;
			}
			ItemBlackBoard itemBlackBoard = itemGeneration.GenerateBlackBoard(this);
			if (!itemBlackBoard.SpawnOnPhase2 && (exIntent || !itemBlackBoard.SpawnInExIntent) && (!exIntent || !itemBlackBoard.RemoveInExIntent))
			{
				int itemGenerateCount = itemBlackBoard.ItemGenerateCount;
				for (int i = 0; i < itemGenerateCount; i++)
				{
					Item item = itemGeneration.GenerateItem(this);
					ApplyContractInitialStrength(item);
					list.Add(item);
				}
			}
		}
		return list;
	}

	public List<Item> GeneratePhase2SpawnItems()
	{
		List<Item> list = new List<Item>();
		foreach (ItemGeneration itemGeneration in sourceDifficultySetup.itemGenerations)
		{
			if (itemGeneration?.item == null)
			{
				continue;
			}
			ItemBlackBoard itemBlackBoard = itemGeneration.GenerateBlackBoard(this);
			if (itemBlackBoard.SpawnOnPhase2 && (exIntent || !itemBlackBoard.SpawnInExIntent) && (!exIntent || !itemBlackBoard.RemoveInExIntent))
			{
				int itemGenerateCount = itemBlackBoard.ItemGenerateCount;
				for (int i = 0; i < itemGenerateCount; i++)
				{
					Item item = itemGeneration.GenerateItem(this);
					ApplyContractInitialStrength(item);
					list.Add(item);
				}
			}
		}
		return list;
	}
}
