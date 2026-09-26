using System;

[Serializable]
public class zbEquipUpgradeSubCsvRecord : zbTableBaseRecord
{
	public int id { get; set; }

	public int lv { get; set; }

	public int equipQuality { get; set; }

	public int exp { get; set; }

	public int deExpTotal { get; set; }

	public int subNum { get; set; }

	public string tempParam1 { get; set; }

	public string tempParam2 { get; set; }

	internal static zbEquipUpgradeSubCsvRecord Read(zbBinReader r)
	{
		return new zbEquipUpgradeSubCsvRecord
		{
			id = r.ReadInt(),
			lv = r.ReadInt(),
			equipQuality = r.ReadInt(),
			exp = r.ReadInt(),
			deExpTotal = r.ReadInt(),
			subNum = r.ReadInt()
		};
	}
}
