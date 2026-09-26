using System;

[Serializable]
public class zbEquipSlotUpgradeCsvRecord : zbTableBaseRecord
{
	public int type { get; set; }

	public int lv { get; set; }

	public int[][] costList { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbEquipSlotUpgradeCsvRecord Read(zbBinReader r)
	{
		return new zbEquipSlotUpgradeCsvRecord
		{
			type = r.ReadInt(),
			lv = r.ReadInt(),
			costList = r.ReadIntArray2()
		};
	}
}
