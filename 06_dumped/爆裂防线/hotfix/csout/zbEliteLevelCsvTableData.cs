using System;
using System.Collections.Generic;

[Serializable]
public class zbEliteLevelCsvTableData
{
	public Dictionary<int, zbEliteLevelCsvRecord> records = new Dictionary<int, zbEliteLevelCsvRecord>();

	public static zbEliteLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEliteLevelCsvTableData zbEliteLevelCsvTableData2 = new zbEliteLevelCsvTableData();
		Dictionary<int, zbEliteLevelCsvRecord> dictionary = new Dictionary<int, zbEliteLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbEliteLevelCsvRecord.Read(zbBinReader2);
		}
		zbEliteLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbEliteLevelCsvTableData2;
	}
}
