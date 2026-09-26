using System;

[Serializable]
public class zbDailyTaskTargetCsvRecord : zbTableBaseRecord
{
	public int active { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDailyTaskTargetCsvRecord Read(zbBinReader r)
	{
		return new zbDailyTaskTargetCsvRecord
		{
			active = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
