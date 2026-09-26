using System;

[Serializable]
public class zbChapterThreeZhanLingCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int openCount { get; set; }

	public int[] lowAward { get; set; }

	public int[] mediumAward { get; set; }

	public int[] highAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterThreeZhanLingCsvRecord Read(zbBinReader r)
	{
		return new zbChapterThreeZhanLingCsvRecord
		{
			level = r.ReadInt(),
			openCount = r.ReadInt(),
			lowAward = r.ReadIntArray(),
			mediumAward = r.ReadIntArray(),
			highAward = r.ReadIntArray()
		};
	}
}
