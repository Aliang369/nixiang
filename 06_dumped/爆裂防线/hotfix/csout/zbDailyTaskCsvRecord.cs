using System;

[Serializable]
public class zbDailyTaskCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int target { get; set; }

	public int active2 { get; set; }

	public int[][] awardList { get; set; }

	public int index { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDailyTaskCsvRecord Read(zbBinReader r)
	{
		return new zbDailyTaskCsvRecord
		{
			type = r.ReadInt(),
			target = r.ReadInt(),
			active2 = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			index = r.ReadInt()
		};
	}
}
