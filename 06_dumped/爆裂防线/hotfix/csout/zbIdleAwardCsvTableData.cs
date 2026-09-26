using System;
using System.Collections.Generic;

[Serializable]
public class zbIdleAwardCsvTableData
{
	public Dictionary<int, zbIdleAwardCsvRecord> records = new Dictionary<int, zbIdleAwardCsvRecord>();

	public static zbIdleAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbIdleAwardCsvTableData zbIdleAwardCsvTableData2 = new zbIdleAwardCsvTableData();
		Dictionary<int, zbIdleAwardCsvRecord> dictionary = new Dictionary<int, zbIdleAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbIdleAwardCsvRecord.Read(zbBinReader2);
		}
		zbIdleAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbIdleAwardCsvTableData2;
	}
}
