using System;

[Serializable]
public class zbZhanLingMainCsvRecord : zbTableBaseRecord
{
	public int mainLevel { get; set; }

	public int[][] lowAward { get; set; }

	public int[][] highAward { get; set; }

	public int goodsId { get; set; }

	public int isGrand { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbZhanLingMainCsvRecord Read(zbBinReader r)
	{
		return new zbZhanLingMainCsvRecord
		{
			mainLevel = r.ReadInt(),
			lowAward = r.ReadIntArray2(),
			highAward = r.ReadIntArray2(),
			goodsId = r.ReadInt(),
			isGrand = r.ReadInt()
		};
	}
}
