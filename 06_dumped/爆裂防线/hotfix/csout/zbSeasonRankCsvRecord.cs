using System;

[Serializable]
public class zbSeasonRankCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int seasonType { get; set; }

	public int settlementType { get; set; }

	public int rankType { get; set; }

	public int rankMin { get; set; }

	public int rankMax { get; set; }

	public int[][] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonRankCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonRankCsvRecord
		{
			id = r.ReadInt(),
			seasonType = r.ReadInt(),
			settlementType = r.ReadInt(),
			rankType = r.ReadInt(),
			rankMin = r.ReadInt(),
			rankMax = r.ReadInt(),
			award = r.ReadIntArray2()
		};
	}
}
