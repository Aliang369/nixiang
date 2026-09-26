using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonCityCsvTableData
{
	public Dictionary<int, zbSeasonCityCsvRecord> records = new Dictionary<int, zbSeasonCityCsvRecord>();

	public static zbSeasonCityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonCityCsvTableData zbSeasonCityCsvTableData2 = new zbSeasonCityCsvTableData();
		Dictionary<int, zbSeasonCityCsvRecord> dictionary = new Dictionary<int, zbSeasonCityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonCityCsvRecord.Read(zbBinReader2);
		}
		zbSeasonCityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonCityCsvTableData2;
	}
}
