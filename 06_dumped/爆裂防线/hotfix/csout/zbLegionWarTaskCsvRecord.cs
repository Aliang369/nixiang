using System;

[Serializable]
public class zbLegionWarTaskCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public int target { get; set; }

	public int[][] awardList { get; set; }

	public string desc { get; set; }

	public int sortIndex { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarTaskCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarTaskCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			target = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			desc = r.ReadString(),
			sortIndex = r.ReadInt()
		};
	}
}
