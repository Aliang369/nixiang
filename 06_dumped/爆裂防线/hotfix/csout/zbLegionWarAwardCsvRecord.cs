using System;

[Serializable]
public class zbLegionWarAwardCsvRecord : zbTableBaseRecord
{
	public int rank { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionWarAwardCsvRecord Read(zbBinReader r)
	{
		return new zbLegionWarAwardCsvRecord
		{
			rank = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
