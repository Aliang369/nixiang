using System;

[Serializable]
public class zbChapterThreeBarTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterThreeBarTargetCsvRecord Read(zbBinReader r)
	{
		return new zbChapterThreeBarTargetCsvRecord
		{
			target = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
