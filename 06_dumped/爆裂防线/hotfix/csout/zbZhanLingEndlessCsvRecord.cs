using System;

[Serializable]
public class zbZhanLingEndlessCsvRecord : zbTableBaseRecord
{
	public int distance { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbZhanLingEndlessCsvRecord Read(zbBinReader r)
	{
		return new zbZhanLingEndlessCsvRecord
		{
			distance = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			isGrand = r.ReadInt()
		};
	}
}
