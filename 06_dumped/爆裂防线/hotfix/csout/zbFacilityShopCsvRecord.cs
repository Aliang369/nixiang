using System;

[Serializable]
public class zbFacilityShopCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int resType { get; set; }

	public int resId { get; set; }

	public int resCount { get; set; }

	public int moneyType { get; set; }

	public float price { get; set; }

	public int weight { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFacilityShopCsvRecord Read(zbBinReader r)
	{
		return new zbFacilityShopCsvRecord
		{
			id = r.ReadInt(),
			resType = r.ReadInt(),
			resId = r.ReadInt(),
			resCount = r.ReadInt(),
			moneyType = r.ReadInt(),
			price = r.ReadFloat(),
			weight = r.ReadInt()
		};
	}
}
