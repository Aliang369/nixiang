using System;

[Serializable]
public class zbSeasonCityLevelCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int[][] awardList { get; set; }

	public int occupyExp { get; set; }

	public int recruitExp { get; set; }

	public int[][] recruitCost { get; set; }

	public int recruitCount { get; set; }

	public int recruitMeritPoint { get; set; }

	public int guardCount { get; set; }

	public int mstLv { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonCityLevelCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonCityLevelCsvRecord
		{
			id = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			occupyExp = r.ReadInt(),
			recruitExp = r.ReadInt(),
			recruitCost = r.ReadIntArray2(),
			recruitCount = r.ReadInt(),
			recruitMeritPoint = r.ReadInt(),
			guardCount = r.ReadInt(),
			mstLv = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt()
		};
	}
}
