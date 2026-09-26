using System;

[Serializable]
public class zbOpsMainQuestCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int[][] awardList { get; set; }

	public int mainLevel { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbOpsMainQuestCsvRecord Read(zbBinReader r)
	{
		return new zbOpsMainQuestCsvRecord
		{
			rank = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			mainLevel = r.ReadInt()
		};
	}
}
