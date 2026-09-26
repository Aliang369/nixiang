using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcDescCsvTableData
{
	public Dictionary<int, zbNpcDescCsvRecord> records = new Dictionary<int, zbNpcDescCsvRecord>();

	public static zbNpcDescCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcDescCsvTableData zbNpcDescCsvTableData2 = new zbNpcDescCsvTableData();
		Dictionary<int, zbNpcDescCsvRecord> dictionary = new Dictionary<int, zbNpcDescCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcDescCsvRecord.Read(zbBinReader2);
		}
		zbNpcDescCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcDescCsvTableData2;
	}
}
