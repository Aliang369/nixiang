using System;

[Serializable]
public class zbAdsZhanLingLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int active { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbAdsZhanLingLevelCsvRecord Read(zbBinReader r)
	{
		return new zbAdsZhanLingLevelCsvRecord
		{
			level = r.ReadInt(),
			active = r.ReadInt()
		};
	}
}
