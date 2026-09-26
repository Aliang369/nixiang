using System;
using System.Collections.Generic;

[Serializable]
public class zbTreasureUpgradeCsvTableData
{
	public Dictionary<int, zbTreasureUpgradeCsvRecord> records = new Dictionary<int, zbTreasureUpgradeCsvRecord>();

	public static zbTreasureUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTreasureUpgradeCsvTableData zbTreasureUpgradeCsvTableData2 = new zbTreasureUpgradeCsvTableData();
		Dictionary<int, zbTreasureUpgradeCsvRecord> dictionary = new Dictionary<int, zbTreasureUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTreasureUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbTreasureUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTreasureUpgradeCsvTableData2;
	}
}
