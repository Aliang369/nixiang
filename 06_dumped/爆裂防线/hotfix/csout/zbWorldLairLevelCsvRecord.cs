using System;

[Serializable]
public class zbWorldLairLevelCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int mainLevel { get; set; }

	public int mstLv { get; set; }

	public int[][] passAwardList { get; set; }

	public int killAwardId { get; set; }

	public int[][] awardShow { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWorldLairLevelCsvRecord Read(zbBinReader r)
	{
		return new zbWorldLairLevelCsvRecord
		{
			lv = r.ReadInt(),
			mainLevel = r.ReadInt(),
			mstLv = r.ReadInt(),
			passAwardList = r.ReadIntArray2(),
			killAwardId = r.ReadInt(),
			awardShow = r.ReadIntArray2()
		};
	}
}
