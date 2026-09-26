using System;

[Serializable]
public class zbMechaSubjectZhanLingCsvRecord : zbTableBaseRecord
{
	public int count { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaSubjectZhanLingCsvRecord Read(zbBinReader r)
	{
		return new zbMechaSubjectZhanLingCsvRecord
		{
			count = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			isGrand = r.ReadInt()
		};
	}
}
