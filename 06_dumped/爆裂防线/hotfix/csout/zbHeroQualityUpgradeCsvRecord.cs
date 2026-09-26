using System;

[Serializable]
public class zbHeroQualityUpgradeCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int qualityStar { get; set; }

	public int[][] costList { get; set; }

	public int costFrag { get; set; }

	public float damage1 { get; set; }

	public float damage2 { get; set; }

	public float damage3 { get; set; }

	public float damage4 { get; set; }

	public float damage10 { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHeroQualityUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbHeroQualityUpgradeCsvRecord
		{
			quality = r.ReadInt(),
			qualityStar = r.ReadInt(),
			costList = r.ReadIntArray2(),
			costFrag = r.ReadInt(),
			damage1 = r.ReadFloat(),
			damage2 = r.ReadFloat(),
			damage3 = r.ReadFloat(),
			damage4 = r.ReadFloat(),
			damage10 = r.ReadFloat()
		};
	}
}
