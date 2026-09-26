using System;

[Serializable]
public class zbPrayLanternAwardCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int floor { get; set; }

	public int[] award { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbPrayLanternAwardCsvRecord Read(zbBinReader r)
	{
		return new zbPrayLanternAwardCsvRecord
		{
			id = r.ReadInt(),
			floor = r.ReadInt(),
			award = r.ReadIntArray()
		};
	}
}
