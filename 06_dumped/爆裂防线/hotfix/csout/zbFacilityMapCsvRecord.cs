using System;

[Serializable]
public class zbFacilityMapCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int type { get; set; }

	public int weight { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFacilityMapCsvRecord Read(zbBinReader r)
	{
		return new zbFacilityMapCsvRecord
		{
			id = r.ReadInt(),
			type = r.ReadInt(),
			weight = r.ReadInt()
		};
	}
}
