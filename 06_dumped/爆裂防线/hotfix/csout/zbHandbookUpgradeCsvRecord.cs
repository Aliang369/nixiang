using System;

[Serializable]
public class zbHandbookUpgradeCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int costItem { get; set; }

	public int[][] costList { get; set; }

	public float damage { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbHandbookUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbHandbookUpgradeCsvRecord
		{
			lv = r.ReadInt(),
			costItem = r.ReadInt(),
			costList = r.ReadIntArray2(),
			damage = r.ReadFloat()
		};
	}
}
