using System;

[Serializable]
public class zbWorldBossRankAwardCsvRecord : zbTableBaseRecord
{
	public int bossid { get; set; }

	public int rank { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldBossRankAwardCsvRecord Read(zbBinReader r)
	{
		return new zbWorldBossRankAwardCsvRecord
		{
			bossid = r.ReadInt(),
			rank = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
