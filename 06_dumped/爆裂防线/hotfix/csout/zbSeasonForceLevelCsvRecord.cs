using System;

[Serializable]
public class zbSeasonForceLevelCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int needExp { get; set; }

	public int[] placeResId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonForceLevelCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonForceLevelCsvRecord
		{
			level = r.ReadInt(),
			needExp = r.ReadInt(),
			placeResId = r.ReadIntArray()
		};
	}
}
