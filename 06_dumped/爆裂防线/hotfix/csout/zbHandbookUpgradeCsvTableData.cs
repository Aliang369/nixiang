using System;
using System.Collections.Generic;

[Serializable]
public class zbHandbookUpgradeCsvTableData
{
	public Dictionary<int, zbHandbookUpgradeCsvRecord> records = new Dictionary<int, zbHandbookUpgradeCsvRecord>();

	public static zbHandbookUpgradeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHandbookUpgradeCsvTableData zbHandbookUpgradeCsvTableData2 = new zbHandbookUpgradeCsvTableData();
		Dictionary<int, zbHandbookUpgradeCsvRecord> dictionary = new Dictionary<int, zbHandbookUpgradeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHandbookUpgradeCsvRecord.Read(zbBinReader2);
		}
		zbHandbookUpgradeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHandbookUpgradeCsvTableData2;
	}
}
