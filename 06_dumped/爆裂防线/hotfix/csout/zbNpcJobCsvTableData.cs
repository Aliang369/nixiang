using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcJobCsvTableData
{
	public Dictionary<int, zbNpcJobCsvRecord> records = new Dictionary<int, zbNpcJobCsvRecord>();

	public static zbNpcJobCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcJobCsvTableData zbNpcJobCsvTableData2 = new zbNpcJobCsvTableData();
		Dictionary<int, zbNpcJobCsvRecord> dictionary = new Dictionary<int, zbNpcJobCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcJobCsvRecord.Read(zbBinReader2);
		}
		zbNpcJobCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcJobCsvTableData2;
	}
}
