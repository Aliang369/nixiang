using System;

[Serializable]
public class zbSeasonAreaCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int mapId { get; set; }

	public string areaName { get; set; }

	public int[] grid { get; set; }

	public int cityId { get; set; }

	public int areaType { get; set; }

	public float[][] validSpawnPos { get; set; }

	public int[] placeResId { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSeasonAreaCsvRecord Read(zbBinReader r)
	{
		return new zbSeasonAreaCsvRecord
		{
			id = r.ReadInt(),
			mapId = r.ReadInt(),
			areaName = r.ReadString(),
			grid = r.ReadIntArray(),
			cityId = r.ReadInt(),
			areaType = r.ReadInt(),
			validSpawnPos = r.ReadFloatArray2(),
			placeResId = r.ReadIntArray()
		};
	}
}
