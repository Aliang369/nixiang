using System;
using System.Collections.Generic;

[Serializable]
public class zbModHandbookCsvTableData
{
	public Dictionary<int, zbModHandbookCsvRecord> records = new Dictionary<int, zbModHandbookCsvRecord>();

	public static zbModHandbookCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbModHandbookCsvTableData zbModHandbookCsvTableData2 = new zbModHandbookCsvTableData();
		Dictionary<int, zbModHandbookCsvRecord> dictionary = new Dictionary<int, zbModHandbookCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbModHandbookCsvRecord.Read(zbBinReader2);
		}
		zbModHandbookCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbModHandbookCsvTableData2;
	}
}
