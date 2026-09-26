using System;

[Serializable]
public class zbWallUpgradeCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int defenseHp { get; set; }

	public int[][] costList { get; set; }

	public int playerLv { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbWallUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbWallUpgradeCsvRecord
		{
			lv = r.ReadInt(),
			defenseHp = r.ReadInt(),
			costList = r.ReadIntArray2(),
			playerLv = r.ReadInt()
		};
	}
}
