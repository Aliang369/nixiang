using System;

[Serializable]
public class zbLegionWarLegionRankCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int[][] winAwardList { get; set; }

	public int[][] failAwardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarLegionRankCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarLegionRankCsvRecord
		{
			rank = r.ReadInt(),
			winAwardList = r.ReadIntArray2(),
			failAwardList = r.ReadIntArray2()
		};
	}
}
