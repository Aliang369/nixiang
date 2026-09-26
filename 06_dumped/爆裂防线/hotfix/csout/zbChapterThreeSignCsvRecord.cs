using System;

[Serializable]
public class zbChapterThreeSignCsvRecord : zbTableBaseRecord
{
	public int day { get; set; }

	public int openCount { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterThreeSignCsvRecord Read(zbBinReader r)
	{
		return new zbChapterThreeSignCsvRecord
		{
			day = r.ReadInt(),
			openCount = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
