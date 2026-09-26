using System;

[Serializable]
public class zbDirectShopCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int[] cost { get; set; }

	public int[][] awardList { get; set; }

	public string bonus { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbDirectShopCsvRecord Read(zbBinReader r)
	{
		return new zbDirectShopCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			cost = r.ReadIntArray(),
			awardList = r.ReadIntArray2(),
			bonus = r.ReadString()
		};
	}
}
