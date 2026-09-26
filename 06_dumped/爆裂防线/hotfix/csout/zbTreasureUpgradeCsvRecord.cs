using System;

[Serializable]
public class zbTreasureUpgradeCsvRecord : zbTableBaseRecord
{
	public int level { get; set; }

	public int costNum { get; set; }

	public int roleLvl { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbTreasureUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbTreasureUpgradeCsvRecord
		{
			level = r.ReadInt(),
			costNum = r.ReadInt(),
			roleLvl = r.ReadInt()
		};
	}
}
