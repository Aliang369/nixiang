using System;

[Serializable]
public class zbHeroFollowCsvRecord : zbTableBaseRecord
{
	public int skillType { get; set; }

	public int lv { get; set; }

	public float damageCoef { get; set; }

	public int[] spcCost { get; set; }

	public int[][] costList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroFollowCsvRecord Read(zbBinReader r)
	{
		return new zbHeroFollowCsvRecord
		{
			skillType = r.ReadInt(),
			lv = r.ReadInt(),
			damageCoef = r.ReadFloat(),
			spcCost = r.ReadIntArray(),
			costList = r.ReadIntArray2()
		};
	}
}
