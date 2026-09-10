using System;

public class CheckCurrentGameDifficulty : CheckNode
{
	public ValueCompareCheckNode.CompareType compareType;

	public DifficultyLevel difficulty;

	public override bool? Check(GameEvent eve)
	{
		if (eve.gameState == null)
		{
			return null;
		}
		int difficultyLevel = (int)eve.gameState.contractState.selectedContract.difficultyLevel;
		int num = (int)difficulty;
		return compareType switch
		{
			ValueCompareCheckNode.CompareType.Equal => difficultyLevel == num, 
			ValueCompareCheckNode.CompareType.Greater => difficultyLevel > num, 
			ValueCompareCheckNode.CompareType.Less => difficultyLevel < num, 
			ValueCompareCheckNode.CompareType.GreaterEqual => difficultyLevel >= num, 
			ValueCompareCheckNode.CompareType.LessEqual => difficultyLevel <= num, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
