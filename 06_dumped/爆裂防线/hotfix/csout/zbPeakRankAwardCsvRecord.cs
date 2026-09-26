using System;

[Serializable]
public class zbPeakRankAwardCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int[][] awardList { get; set; }

	public int[] bestAward { get; set; }

	public int showAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPeakRankAwardCsvRecord Read(zbBinReader r)
	{
		return new zbPeakRankAwardCsvRecord
		{
			rank = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			bestAward = r.ReadIntArray(),
			showAward = r.ReadInt()
		};
	}
}
