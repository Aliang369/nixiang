public class Season
{
	[SaveField(null, null)]
	public SeasonDef def;

	[SaveField(null, null)]
	public LevelLine seasonLeveLine;

	[SaveField(null, null)]
	public bool seasonPassUnlocked;

	[SaveField(null, null)]
	public bool highseasonPassUnlocked;

	private Season()
	{
	}

	public Season(SeasonDef def)
	{
		this.def = def;
		seasonLeveLine = new LevelLine(def.levelLine);
	}

	public void UnlockSeasonPass(ApplicationStateInfo applicationState)
	{
		if (!seasonPassUnlocked)
		{
			seasonPassUnlocked = true;
			applicationState.savingManager.SetProfileDirtyToSave();
		}
	}

	public void UnlockHighSeasonPass(ApplicationStateInfo applicationState)
	{
		if (!highseasonPassUnlocked)
		{
			highseasonPassUnlocked = true;
			applicationState.savingManager.SetProfileDirtyToSave();
		}
	}
}
