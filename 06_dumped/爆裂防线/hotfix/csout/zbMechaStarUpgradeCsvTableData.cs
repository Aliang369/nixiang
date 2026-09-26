using System;
using System.Collections.Generic;

[Serializable]
public class zbMechaStarUpgradeCsvTableData
{
	public Dictionary<int, zbMechaStarUpgradeCsvRecord> records = new Dictionary<int, zbMechaStarUpgradeCsvRecord>();

	public static zbMechaStarUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMechaStarUpgradeCsvTableData zbMechaStarUpgradeCsvTableData2 = new zbMechaStarUpgradeCsvTableData();
		Dictionary<int, zbMechaStarUpgradeCsvRecord> dictionary = new Dictionary<int, zbMechaStarUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMechaStarUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbMechaStarUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMechaStarUpgradeCsvTableData2;
	}
}
