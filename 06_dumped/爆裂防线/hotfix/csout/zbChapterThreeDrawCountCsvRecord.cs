using System;

[Serializable]
public class zbChapterThreeDrawCountCsvRecord : zbTableBaseRecord
{
	public int index { get; set; }

	public int target { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterThreeDrawCountCsvRecord Read(zbBinReader r)
	{
		return new zbChapterThreeDrawCountCsvRecord
		{
			index = r.ReadInt(),
			target = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
