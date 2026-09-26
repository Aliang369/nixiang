using System;

[Serializable]
public class zbZhanLingKillTargetCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int kill { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbZhanLingKillTargetCsvRecord Read(zbBinReader r)
	{
		return new zbZhanLingKillTargetCsvRecord
		{
			lv = r.ReadInt(),
			kill = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			isGrand = r.ReadInt()
		};
	}
}
