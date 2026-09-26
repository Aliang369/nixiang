using System;

[Serializable]
public class zbChapterOneBarTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterOneBarTargetCsvRecord Read(zbBinReader r)
	{
		return new zbChapterOneBarTargetCsvRecord
		{
			target = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
