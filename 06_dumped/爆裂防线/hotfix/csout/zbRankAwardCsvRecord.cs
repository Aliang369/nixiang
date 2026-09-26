using System;

[Serializable]
public class zbRankAwardCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int id { get; set; }

	public int value { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRankAwardCsvRecord Read(zbBinReader r)
	{
		return new zbRankAwardCsvRecord
		{
			type = r.ReadInt(),
			id = r.ReadInt(),
			value = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
