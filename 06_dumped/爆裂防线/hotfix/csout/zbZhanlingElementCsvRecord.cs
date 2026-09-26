using System;

[Serializable]
public class zbZhanlingElementCsvRecord : zbTableBaseRecord
{
	public int trialLevel { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbZhanlingElementCsvRecord Read(zbBinReader r)
	{
		return new zbZhanlingElementCsvRecord
		{
			trialLevel = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			isGrand = r.ReadInt()
		};
	}
}
