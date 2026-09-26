using System;

[Serializable]
public class zbChapterTwoDiamondCostCsvRecord : zbTableBaseRecord
{
	public int index { get; set; }

	public int diamondCost { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbChapterTwoDiamondCostCsvRecord Read(zbBinReader r)
	{
		return new zbChapterTwoDiamondCostCsvRecord
		{
			index = r.ReadInt(),
			diamondCost = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
