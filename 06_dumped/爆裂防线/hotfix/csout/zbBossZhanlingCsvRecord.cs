using System;

[Serializable]
public class zbBossZhanlingCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int score { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbBossZhanlingCsvRecord Read(zbBinReader r)
	{
		return new zbBossZhanlingCsvRecord
		{
			lv = r.ReadInt(),
			score = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			isGrand = r.ReadInt()
		};
	}
}
