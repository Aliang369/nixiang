using System;
using System.Collections.Generic;

[Serializable]
public class zbModHandbookTypeCsvTableData
{
	public Dictionary<int, zbModHandbookTypeCsvRecord> records = new Dictionary<int, zbModHandbookTypeCsvRecord>();

	public static zbModHandbookTypeCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbModHandbookTypeCsvTableData zbModHandbookTypeCsvTableData2 = new zbModHandbookTypeCsvTableData();
		Dictionary<int, zbModHandbookTypeCsvRecord> dictionary = new Dictionary<int, zbModHandbookTypeCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbModHandbookTypeCsvRecord.Read(zbBinReader2);
		}
		zbModHandbookTypeCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbModHandbookTypeCsvTableData2;
	}
}
