using System;

[Serializable]
public class zbLegionWarPlayerRankCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int[][] winAwardList { get; set; }

	public int[][] failAwardList { get; set; }

	public string showRank { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarPlayerRankCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarPlayerRankCsvRecord
		{
			rank = r.ReadInt(),
			winAwardList = r.ReadIntArray2(),
			failAwardList = r.ReadIntArray2(),
			showRank = r.ReadString()
		};
	}
}
