using System;

[Serializable]
public class zbTruckQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int arriveTime { get; set; }

	public int points { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTruckQualityCsvRecord Read(zbBinReader r)
	{
		return new zbTruckQualityCsvRecord
		{
			quality = r.ReadInt(),
			arriveTime = r.ReadInt(),
			points = r.ReadInt()
		};
	}
}
