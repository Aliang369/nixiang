using System;

[Serializable]
public class zbSubscribeShopCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int price { get; set; }

	public int[][] awardList { get; set; }

	public int sort { get; set; }

	public int refresh { get; set; }

	public int limitNum { get; set; }

	public string bonus { get; set; }

	public int isValid { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbSubscribeShopCsvRecord Read(zbBinReader r)
	{
		return new zbSubscribeShopCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			price = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			sort = r.ReadInt(),
			refresh = r.ReadInt(),
			limitNum = r.ReadInt(),
			bonus = r.ReadString(),
			isValid = r.ReadInt()
		};
	}
}
