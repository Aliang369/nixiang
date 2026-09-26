using System;
using System.Collections.Generic;

[Serializable]
public class zbStrongerCsvTableData
{
	public Dictionary<int, zbStrongerCsvRecord> records = new Dictionary<int, zbStrongerCsvRecord>();

	public static zbStrongerCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbStrongerCsvTableData zbStrongerCsvTableData2 = new zbStrongerCsvTableData();
		Dictionary<int, zbStrongerCsvRecord> dictionary = new Dictionary<int, zbStrongerCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbStrongerCsvRecord.Read(zbBinReader2);
		}
		zbStrongerCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbStrongerCsvTableData2;
	}
}
