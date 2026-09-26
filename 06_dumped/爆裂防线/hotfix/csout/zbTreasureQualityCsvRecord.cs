using System;

[Serializable]
public class zbTreasureQualityCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int maxLvl { get; set; }

	public int[] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTreasureQualityCsvRecord Read(zbBinReader r)
	{
		return new zbTreasureQualityCsvRecord
		{
			quality = r.ReadInt(),
			maxLvl = r.ReadInt(),
			awardList = r.ReadIntArray()
		};
	}
}
