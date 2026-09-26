using System;
using System.Collections.Generic;

[Serializable]
public class zbMainLevelCsvTableData
{
	public Dictionary<int, zbMainLevelCsvRecord> records = new Dictionary<int, zbMainLevelCsvRecord>();

	public static zbMainLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMainLevelCsvTableData zbMainLevelCsvTableData2 = new zbMainLevelCsvTableData();
		Dictionary<int, zbMainLevelCsvRecord> dictionary = new Dictionary<int, zbMainLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMainLevelCsvRecord.Read(zbBinReader2);
		}
		zbMainLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMainLevelCsvTableData2;
	}
}
