using System.Collections.Generic;

public class ClassMatterManager : InGameManager
{
	private MatterDef currentDef => base.gameState.playerState.CharacterDef.attachClass.classMatter;

	public int currentEnteredNodePoint
	{
		get
		{
			int num = 0;
			for (int i = 1; i < base.gameState.contractState.stage; i++)
			{
				num++;
			}
			PathLocation currentLocation = base.gameState.dungeonMapManager.currentLocation;
			if (currentLocation != null && currentLocation.isNodePoint)
			{
				num++;
			}
			return num;
		}
	}

	public Dictionary<DifficultyLevel, Dictionary<int, int>> config => ScriptableObjectSingleton<FortressDataManager>.instance.stageClassMatterConfig;

	public MatterInfo GetStageClassMatterInfo(int stage)
	{
		return new MatterInfo(currentDef, GetStageClassMatterCount(stage));
	}

	public MatterInfo GenerateGameOverMatters(int enteredNodePoint)
	{
		int num = GetTotalMatterCount(enteredNodePoint);
		int variableValue = base.gameState.globalVariableStateInfo.GetVariableValue(InGameGlobalVariableStateInfo.ClassMatterRewardMultiplier);
		if (variableValue > 1)
		{
			num *= variableValue;
		}
		if (num <= 0)
		{
			return null;
		}
		return new MatterInfo(currentDef, num);
	}

	private int GetTotalMatterCount(int enteredNodePoint)
	{
		int num = 0;
		for (int i = 1; i <= enteredNodePoint; i++)
		{
			num += GetStageClassMatterCount(i);
		}
		return num;
	}

	private int GetStageClassMatterCount(int stage)
	{
		DifficultyLevel? difficultyLevel = base.gameState?.contractState?.selectedContract?.difficultyLevel;
		if (!difficultyLevel.HasValue || !config.ContainsKey(difficultyLevel.Value) || !config[difficultyLevel.Value].ContainsKey(stage))
		{
			return 0;
		}
		return config[difficultyLevel.Value][stage];
	}
}
