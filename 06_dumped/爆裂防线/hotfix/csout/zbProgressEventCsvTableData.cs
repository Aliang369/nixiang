using System;
using System.Collections.Generic;

[Serializable]
public class zbProgressEventCsvTableData
{
	public Dictionary<int, zbProgressEventCsvRecord> records = new Dictionary<int, zbProgressEventCsvRecord>();

	public static zbProgressEventCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbProgressEventCsvTableData zbProgressEventCsvTableData2 = new zbProgressEventCsvTableData();
		Dictionary<int, zbProgressEventCsvRecord> dictionary = new Dictionary<int, zbProgressEventCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbProgressEventCsvRecord.Read(zbBinReader2);
		}
		zbProgressEventCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbProgressEventCsvTableData2;
	}
}
