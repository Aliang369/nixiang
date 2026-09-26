using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcBodyColorCsvTableData
{
	public Dictionary<int, zbNpcBodyColorCsvRecord> records = new Dictionary<int, zbNpcBodyColorCsvRecord>();

	public static zbNpcBodyColorCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcBodyColorCsvTableData zbNpcBodyColorCsvTableData2 = new zbNpcBodyColorCsvTableData();
		Dictionary<int, zbNpcBodyColorCsvRecord> dictionary = new Dictionary<int, zbNpcBodyColorCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcBodyColorCsvRecord.Read(zbBinReader2);
		}
		zbNpcBodyColorCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcBodyColorCsvTableData2;
	}
}
