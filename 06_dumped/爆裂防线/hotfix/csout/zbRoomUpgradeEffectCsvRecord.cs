using System;

[Serializable]
public class zbRoomUpgradeEffectCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int lv { get; set; }

	public int costItemNum { get; set; }

	public int[][] costList { get; set; }

	public int playerLv { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbRoomUpgradeEffectCsvRecord Read(zbBinReader r)
	{
		return new zbRoomUpgradeEffectCsvRecord
		{
			id = r.ReadInt(),
			lv = r.ReadInt(),
			costItemNum = r.ReadInt(),
			costList = r.ReadIntArray2(),
			playerLv = r.ReadInt()
		};
	}
}
