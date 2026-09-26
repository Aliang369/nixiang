using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldEventFightCsvTableData
{
	public Dictionary<int, zbWorldEventFightCsvRecord> records = new Dictionary<int, zbWorldEventFightCsvRecord>();

	public static zbWorldEventFightCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldEventFightCsvTableData zbWorldEventFightCsvTableData2 = new zbWorldEventFightCsvTableData();
		Dictionary<int, zbWorldEventFightCsvRecord> dictionary = new Dictionary<int, zbWorldEventFightCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldEventFightCsvRecord.Read(zbBinReader2);
		}
		zbWorldEventFightCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldEventFightCsvTableData2;
	}
}
