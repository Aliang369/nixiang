using System;

[Serializable]
public class zbVipDailyPackCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public string name { get; set; }

	public int type { get; set; }

	public int[][] awardList { get; set; }

	public int goodsId { get; set; }

	public string bonus { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipDailyPackCsvRecord Read(zbBinReader r)
	{
		return new zbVipDailyPackCsvRecord
		{
			id = r.ReadInt(),
			name = r.ReadString(),
			type = r.ReadInt(),
			awardList = r.ReadIntArray2(),
			goodsId = r.ReadInt(),
			bonus = r.ReadString()
		};
	}
}
