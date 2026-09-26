using System;
using System.Collections.Generic;

[Serializable]
public class zbWallUpgradeCsvTableData
{
	public Dictionary<int, zbWallUpgradeCsvRecord> records = new Dictionary<int, zbWallUpgradeCsvRecord>();

	public static zbWallUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWallUpgradeCsvTableData zbWallUpgradeCsvTableData2 = new zbWallUpgradeCsvTableData();
		Dictionary<int, zbWallUpgradeCsvRecord> dictionary = new Dictionary<int, zbWallUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWallUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbWallUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWallUpgradeCsvTableData2;
	}
}
