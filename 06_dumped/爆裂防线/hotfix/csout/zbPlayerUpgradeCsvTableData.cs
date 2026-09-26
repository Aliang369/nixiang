using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerUpgradeCsvTableData
{
	public Dictionary<int, zbPlayerUpgradeCsvRecord> records = new Dictionary<int, zbPlayerUpgradeCsvRecord>();

	public static zbPlayerUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerUpgradeCsvTableData zbPlayerUpgradeCsvTableData2 = new zbPlayerUpgradeCsvTableData();
		Dictionary<int, zbPlayerUpgradeCsvRecord> dictionary = new Dictionary<int, zbPlayerUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbPlayerUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerUpgradeCsvTableData2;
	}
}
