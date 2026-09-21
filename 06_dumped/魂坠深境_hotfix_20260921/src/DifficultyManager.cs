public class DifficultyManager : GlobalGameManager
{
	[SaveField(null, null)]
	public new ApplicationStateInfo applicationState;

	[SaveField(null, null)]
	public DifficultyLevel finishedMaxDifficulty;

	[SaveField(null, null)]
	private bool initDifficultySet;

	public DifficultyLevel MaxUnlockDifficulty
	{
		get
		{
			if (finishedMaxDifficulty < DifficultyLevel.Normal)
			{
				return DifficultyLevel.Normal;
			}
			return finishedMaxDifficulty + 1;
		}
	}

	public override void Init(ApplicationStateInfo applicationState)
	{
		this.applicationState = applicationState;
		if (!initDifficultySet)
		{
			finishedMaxDifficulty = DifficultyLevel.Not;
			initDifficultySet = true;
		}
	}

	public DifficultyInfo GetDifficultyInfo(DifficultyLevel difficulty)
	{
		return applicationState.assetCatalog.difficultyCatalog.ValidAssets.Find((DifficultyInfo d) => d.difficultyLevel == difficulty);
	}

	public bool IsDifficultyUnlocked(DifficultyLevel level)
	{
		if (level <= MaxUnlockDifficulty)
		{
			return true;
		}
		return false;
	}

	public bool IsDifficultyFinished(DifficultyLevel level)
	{
		if (level <= finishedMaxDifficulty)
		{
			return true;
		}
		return false;
	}

	public void FinishDifficulty(DifficultyLevel level)
	{
		if (level >= finishedMaxDifficulty)
		{
			finishedMaxDifficulty = level;
		}
		applicationState.savingManager.SetProfileDirtyToSave();
	}
}
