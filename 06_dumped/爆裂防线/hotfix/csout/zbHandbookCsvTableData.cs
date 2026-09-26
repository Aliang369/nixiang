using System;
using System.Collections.Generic;

[Serializable]
public class zbHandbookCsvTableData
{
	public Dictionary<int, zbHandbookCsvRecord> records = new Dictionary<int, zbHandbookCsvRecord>();

	public static zbHandbookCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHandbookCsvTableData zbHandbookCsvTableData2 = new zbHandbookCsvTableData();
		Dictionary<int, zbHandbookCsvRecord> dictionary = new Dictionary<int, zbHandbookCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHandbookCsvRecord.Read(zbBinReader2);
		}
		zbHandbookCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHandbookCsvTableData2;
	}
}
