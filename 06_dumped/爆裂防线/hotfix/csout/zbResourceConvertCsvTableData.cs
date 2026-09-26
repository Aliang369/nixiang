using System;
using System.Collections.Generic;

[Serializable]
public class zbResourceConvertCsvTableData
{
	public Dictionary<int, zbResourceConvertCsvRecord> records = new Dictionary<int, zbResourceConvertCsvRecord>();

	public static zbResourceConvertCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbResourceConvertCsvTableData zbResourceConvertCsvTableData2 = new zbResourceConvertCsvTableData();
		Dictionary<int, zbResourceConvertCsvRecord> dictionary = new Dictionary<int, zbResourceConvertCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbResourceConvertCsvRecord.Read(zbBinReader2);
		}
		zbResourceConvertCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbResourceConvertCsvTableData2;
	}
}
