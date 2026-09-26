using System;

[Serializable]
public class zbChapterOneSignCsvRecord : zbTableBaseRecord
{
	public int day { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterOneSignCsvRecord Read(zbBinReader r)
	{
		return new zbChapterOneSignCsvRecord
		{
			day = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
