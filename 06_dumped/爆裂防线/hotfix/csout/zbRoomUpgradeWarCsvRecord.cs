using System;

[Serializable]
public class zbRoomUpgradeWarCsvRecord : zbTableBaseRecord
{
	public int lv { get; set; }

	public int costItemNum { get; set; }

	public int[][] costList { get; set; }

	public int playerLv { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRoomUpgradeWarCsvRecord Read(zbBinReader r)
	{
		return new zbRoomUpgradeWarCsvRecord
		{
			lv = r.ReadInt(),
			costItemNum = r.ReadInt(),
			costList = r.ReadIntArray2(),
			playerLv = r.ReadInt()
		};
	}
}
