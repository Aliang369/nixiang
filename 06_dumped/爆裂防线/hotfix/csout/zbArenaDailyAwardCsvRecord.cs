using System;

[Serializable]
public class zbArenaDailyAwardCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int rankMoney { get; set; }

	public string showRank { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbArenaDailyAwardCsvRecord Read(zbBinReader r)
	{
		return new zbArenaDailyAwardCsvRecord
		{
			rank = r.ReadInt(),
			rankMoney = r.ReadInt(),
			showRank = r.ReadString()
		};
	}
}
