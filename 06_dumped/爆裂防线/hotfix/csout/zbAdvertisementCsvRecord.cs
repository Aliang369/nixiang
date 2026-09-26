using System;

[Serializable]
public class zbAdvertisementCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public int refresh { get; set; }

	public int limitNum { get; set; }

	public int[][] awardList { get; set; }

	public int admobType { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbAdvertisementCsvRecord Read(zbBinReader r)
	{
		return new zbAdvertisementCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			refresh = r.ReadInt(),
			limitNum = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			admobType = r.ReadInt()
		};
	}
}
