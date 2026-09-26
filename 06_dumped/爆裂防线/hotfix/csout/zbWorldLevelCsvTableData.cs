using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldLevelCsvTableData
{
	public Dictionary<int, zbWorldLevelCsvRecord> records = new Dictionary<int, zbWorldLevelCsvRecord>();

	public static zbWorldLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldLevelCsvTableData zbWorldLevelCsvTableData2 = new zbWorldLevelCsvTableData();
		Dictionary<int, zbWorldLevelCsvRecord> dictionary = new Dictionary<int, zbWorldLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldLevelCsvRecord.Read(zbBinReader2);
		}
		zbWorldLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldLevelCsvTableData2;
	}
}
