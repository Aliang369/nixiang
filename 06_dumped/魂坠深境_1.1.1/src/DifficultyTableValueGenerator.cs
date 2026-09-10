using UnityEngine;

public class DifficultyTableValueGenerator : TableValueGenerator
{
	public enum KeySource
	{
		Manual,
		Difficulty
	}

	[SerializeField]
	private KeySource rowSource;

	[SerializeField]
	private KeySource colSource;

	[SerializeField]
	private object manualRow;

	[SerializeField]
	private object manualCol;

	protected override object GetRowKey(GameEvent eve)
	{
		if (rowSource == KeySource.Difficulty)
		{
			return eve.gameState.contractState.selectedContract.difficultyLevel;
		}
		return manualRow;
	}

	protected override object GetColKey(GameEvent eve)
	{
		if (colSource == KeySource.Difficulty)
		{
			return eve.gameState.contractState.selectedContract.difficultyLevel;
		}
		return manualCol;
	}
}
