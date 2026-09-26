using System;
using System.Collections.Generic;

[Serializable]
public class zbItemCsvTableData
{
	public Dictionary<int, zbItemCsvRecord> records = new Dictionary<int, zbItemCsvRecord>();

	public static zbItemCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbItemCsvTableData zbItemCsvTableData2 = new zbItemCsvTableData();
		Dictionary<int, zbItemCsvRecord> dictionary = new Dictionary<int, zbItemCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbItemCsvRecord.Read(zbBinReader2);
		}
		zbItemCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbItemCsvTableData2;
	}
}
