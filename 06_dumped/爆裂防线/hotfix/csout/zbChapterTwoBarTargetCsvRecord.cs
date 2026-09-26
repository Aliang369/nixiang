using System;

[Serializable]
public class zbChapterTwoBarTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterTwoBarTargetCsvRecord Read(zbBinReader r)
	{
		return new zbChapterTwoBarTargetCsvRecord
		{
			target = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
