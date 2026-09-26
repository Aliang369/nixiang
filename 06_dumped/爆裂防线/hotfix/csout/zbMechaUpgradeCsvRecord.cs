using System;

[Serializable]
public class zbMechaUpgradeCsvRecord : zbTableBaseRecord
{
	public int quality { get; set; }

	public int lv { get; set; }

	public int costNum { get; set; }

	public int[][] costList { get; set; }

	public int attack { get; set; }

	public int playerLv { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbMechaUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbMechaUpgradeCsvRecord
		{
			quality = r.ReadInt(),
			lv = r.ReadInt(),
			costNum = r.ReadInt(),
			costList = r.ReadIntArray2(),
			attack = r.ReadInt(),
			playerLv = r.ReadInt()
		};
	}
}
