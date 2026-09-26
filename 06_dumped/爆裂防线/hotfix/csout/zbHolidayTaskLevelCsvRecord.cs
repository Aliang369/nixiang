using System;

[Serializable]
public class zbHolidayTaskLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int[][] awardLow { get; set; }

	public int[][] awardHigh { get; set; }

	public int actAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHolidayTaskLevelCsvRecord Read(zbBinReader r)
	{
		return new zbHolidayTaskLevelCsvRecord
		{
			level = r.ReadInt(),
			awardLow = r.ReadIntArray2(),
			awardHigh = r.ReadIntArray2(),
			actAward = r.ReadInt()
		};
	}
}
