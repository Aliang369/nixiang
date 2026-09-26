using System;

[Serializable]
public class zbChapterTwoZhanLingCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int count { get; set; }

	public int[] lowAward { get; set; }

	public int[] mediumAward { get; set; }

	public int[] highAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterTwoZhanLingCsvRecord Read(zbBinReader r)
	{
		return new zbChapterTwoZhanLingCsvRecord
		{
			level = r.ReadInt(),
			count = r.ReadInt(),
			lowAward = r.ReadIntArray(),
			mediumAward = r.ReadIntArray(),
			highAward = r.ReadIntArray()
		};
	}
}
