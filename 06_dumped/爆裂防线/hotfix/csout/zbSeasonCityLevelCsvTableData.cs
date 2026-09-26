using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonCityLevelCsvTableData
{
	public Dictionary<int, zbSeasonCityLevelCsvRecord> records = new Dictionary<int, zbSeasonCityLevelCsvRecord>();

	public static zbSeasonCityLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonCityLevelCsvTableData zbSeasonCityLevelCsvTableData2 = new zbSeasonCityLevelCsvTableData();
		Dictionary<int, zbSeasonCityLevelCsvRecord> dictionary = new Dictionary<int, zbSeasonCityLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonCityLevelCsvRecord.Read(zbBinReader2);
		}
		zbSeasonCityLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonCityLevelCsvTableData2;
	}
}
