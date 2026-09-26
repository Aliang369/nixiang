using System;

[Serializable]
public class zbFacilityCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int quality { get; set; }

	public string name { get; set; }

	public int createCostItem { get; set; }

	public int duration { get; set; }

	public int[][] gridSpec { get; set; }

	public int shareCount { get; set; }

	public int creatorExploreLimit { get; set; }

	public int tierTarget1 { get; set; }

	public int[][] tierAwardList1 { get; set; }

	public int tierTarget2 { get; set; }

	public int[][] tierAwardList2 { get; set; }

	public int tierTarget3 { get; set; }

	public int[][] tierAwardList3 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbFacilityCsvRecord Read(zbBinReader r)
	{
		return new zbFacilityCsvRecord
		{
			type = r.ReadInt(),
			quality = r.ReadInt(),
			name = r.ReadString(),
			createCostItem = r.ReadInt(),
			duration = r.ReadInt(),
			gridSpec = r.ReadIntArray2(),
			shareCount = r.ReadInt(),
			creatorExploreLimit = r.ReadInt(),
			tierTarget1 = r.ReadInt(),
			tierAwardList1 = r.ReadIntArray2(),
			tierTarget2 = r.ReadInt(),
			tierAwardList2 = r.ReadIntArray2(),
			tierTarget3 = r.ReadInt(),
			tierAwardList3 = r.ReadIntArray2()
		};
	}
}
