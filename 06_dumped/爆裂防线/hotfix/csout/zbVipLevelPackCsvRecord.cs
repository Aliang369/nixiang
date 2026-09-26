using System;

[Serializable]
public class zbVipLevelPackCsvRecord : zbTableBaseRecord
{
	public int lvl { get; set; }

	public int goodsId { get; set; }

	public string offShow { get; set; }

	public int[][] dailyFreeAward { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbVipLevelPackCsvRecord Read(zbBinReader r)
	{
		return new zbVipLevelPackCsvRecord
		{
			lvl = r.ReadInt(),
			goodsId = r.ReadInt(),
			offShow = r.ReadString(),
			dailyFreeAward = r.ReadIntArray2()
		};
	}
}
