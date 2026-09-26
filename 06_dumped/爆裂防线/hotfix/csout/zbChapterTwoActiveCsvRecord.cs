using System;

[Serializable]
public class zbChapterTwoActiveCsvRecord : zbTableBaseRecord
{
	public int active { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterTwoActiveCsvRecord Read(zbBinReader r)
	{
		return new zbChapterTwoActiveCsvRecord
		{
			active = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
