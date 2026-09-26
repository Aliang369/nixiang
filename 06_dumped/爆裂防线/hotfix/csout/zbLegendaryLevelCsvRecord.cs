using System;

[Serializable]
public class zbLegendaryLevelCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int mainLevel { get; set; }

	public int sceneId { get; set; }

	public int mstLv { get; set; }

	public int mstPid1 { get; set; }

	public int mstPid2 { get; set; }

	public int mstPid3 { get; set; }

	public int mstPid4 { get; set; }

	public int mstPid5 { get; set; }

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

	internal static zbLegendaryLevelCsvRecord Read(zbBinReader r)
	{
		return new zbLegendaryLevelCsvRecord
		{
			id = r.ReadInt(),
			mainLevel = r.ReadInt(),
			sceneId = r.ReadInt(),
			mstLv = r.ReadInt(),
			mstPid1 = r.ReadInt(),
			mstPid2 = r.ReadInt(),
			mstPid3 = r.ReadInt(),
			mstPid4 = r.ReadInt(),
			mstPid5 = r.ReadInt(),
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
