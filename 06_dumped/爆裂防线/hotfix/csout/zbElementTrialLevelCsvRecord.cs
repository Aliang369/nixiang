using System;

[Serializable]
public class zbElementTrialLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int mainQuestOpen { get; set; }

	public int unlockTotal { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbElementTrialLevelCsvRecord Read(zbBinReader r)
	{
		return new zbElementTrialLevelCsvRecord
		{
			level = r.ReadInt(),
			mainQuestOpen = r.ReadInt(),
			unlockTotal = r.ReadInt()
		};
	}
}
