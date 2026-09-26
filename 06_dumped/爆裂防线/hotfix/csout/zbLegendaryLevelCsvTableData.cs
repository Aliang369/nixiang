using System;
using System.Collections.Generic;

[Serializable]
public class zbLegendaryLevelCsvTableData
{
	public Dictionary<int, zbLegendaryLevelCsvRecord> records = new Dictionary<int, zbLegendaryLevelCsvRecord>();

	public static zbLegendaryLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegendaryLevelCsvTableData zbLegendaryLevelCsvTableData2 = new zbLegendaryLevelCsvTableData();
		Dictionary<int, zbLegendaryLevelCsvRecord> dictionary = new Dictionary<int, zbLegendaryLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegendaryLevelCsvRecord.Read(zbBinReader2);
		}
		zbLegendaryLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegendaryLevelCsvTableData2;
	}
}
