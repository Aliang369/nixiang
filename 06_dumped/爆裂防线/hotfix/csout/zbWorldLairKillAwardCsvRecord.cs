using System;

[Serializable]
public class zbWorldLairKillAwardCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int killCount { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldLairKillAwardCsvRecord Read(zbBinReader r)
	{
		return new zbWorldLairKillAwardCsvRecord
		{
			id = r.ReadInt(),
			killCount = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
