using System;
using System.Collections.Generic;

[Serializable]
public class zbMineNodeCsvTableData
{
	public Dictionary<int, zbMineNodeCsvRecord> records = new Dictionary<int, zbMineNodeCsvRecord>();

	public static zbMineNodeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMineNodeCsvTableData zbMineNodeCsvTableData2 = new zbMineNodeCsvTableData();
		Dictionary<int, zbMineNodeCsvRecord> dictionary = new Dictionary<int, zbMineNodeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMineNodeCsvRecord.Read(zbBinReader2);
		}
		zbMineNodeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMineNodeCsvTableData2;
	}
}
