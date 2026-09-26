using System;

[Serializable]
public class zbLegionDonateTargetCsvRecord : zbTableBaseRecord
{
	public int target { get; set; }

	public int[][] awardList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbLegionDonateTargetCsvRecord Read(zbBinReader r)
	{
		return new zbLegionDonateTargetCsvRecord
		{
			target = r.ReadInt(),
			awardList = r.ReadIntArray2()
		};
	}
}
