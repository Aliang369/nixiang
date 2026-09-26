using System;

[Serializable]
public class zbMainLevelCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int sceneId { get; set; }

	public string mainScene { get; set; }

	public int mstLv { get; set; }

	public int mst1 { get; set; }

	public int mst2 { get; set; }

	public int mst3 { get; set; }

	public int mst4 { get; set; }

	public int mst5 { get; set; }

	public int[][] awardStar1 { get; set; }

	public int[][] awardStar2 { get; set; }

	public int[][] awardStar3 { get; set; }

	public int[][] awardShow { get; set; }

	public int[][] baseAward { get; set; }

	public int[][] growthAward { get; set; }

	public int[][] spAward1 { get; set; }

	public int spLvl1 { get; set; }

	public int[][] spAward2 { get; set; }

	public int spLvl2 { get; set; }

	public int[][] spAward3 { get; set; }

	public int spLvl3 { get; set; }

	public int[][] spAward4 { get; set; }

	public int spLvl4 { get; set; }

	public int[][] winAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMainLevelCsvRecord Read(zbBinReader r)
	{
		return new zbMainLevelCsvRecord
		{
			id = r.ReadInt(),
			sceneId = r.ReadInt(),
			mainScene = r.ReadString(),
			mstLv = r.ReadInt(),
			mst1 = r.ReadInt(),
			mst2 = r.ReadInt(),
			mst3 = r.ReadInt(),
			mst4 = r.ReadInt(),
			mst5 = r.ReadInt(),
			awardStar1 = r.ReadIntArray2(),
			awardStar2 = r.ReadIntArray2(),
			awardStar3 = r.ReadIntArray2(),
			awardShow = r.ReadIntArray2(),
			baseAward = r.ReadIntArray2(),
			growthAward = r.ReadIntArray2(),
			spAward1 = r.ReadIntArray2(),
			spLvl1 = r.ReadInt(),
			spAward2 = r.ReadIntArray2(),
			spLvl2 = r.ReadInt(),
			spAward3 = r.ReadIntArray2(),
			spLvl3 = r.ReadInt(),
			spAward4 = r.ReadIntArray2(),
			spLvl4 = r.ReadInt(),
			winAward = r.ReadIntArray2()
		};
	}
}
