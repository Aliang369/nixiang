using System;

[Serializable]
public class zbZhanLingLegendaryCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int goodsId { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbZhanLingLegendaryCsvRecord Read(zbBinReader r)
	{
		return new zbZhanLingLegendaryCsvRecord
		{
			lv = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			goodsId = r.ReadInt(),
			isGrand = r.ReadInt()
		};
	}
}
