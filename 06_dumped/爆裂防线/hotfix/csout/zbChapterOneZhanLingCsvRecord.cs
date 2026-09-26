using System;

[Serializable]
public class zbChapterOneZhanLingCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int killCount { get; set; }

	public int[] lowAward { get; set; }

	public int[] mediumAward { get; set; }

	public int[] highAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterOneZhanLingCsvRecord Read(zbBinReader r)
	{
		return new zbChapterOneZhanLingCsvRecord
		{
			level = r.ReadInt(),
			killCount = r.ReadInt(),
			lowAward = r.ReadIntArray(),
			mediumAward = r.ReadIntArray(),
			highAward = r.ReadIntArray()
		};
	}
}
