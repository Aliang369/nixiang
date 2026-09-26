using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonForceLevelCsvTableData
{
	public Dictionary<int, zbSeasonForceLevelCsvRecord> records = new Dictionary<int, zbSeasonForceLevelCsvRecord>();

	public static zbSeasonForceLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonForceLevelCsvTableData zbSeasonForceLevelCsvTableData2 = new zbSeasonForceLevelCsvTableData();
		Dictionary<int, zbSeasonForceLevelCsvRecord> dictionary = new Dictionary<int, zbSeasonForceLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonForceLevelCsvRecord.Read(zbBinReader2);
		}
		zbSeasonForceLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonForceLevelCsvTableData2;
	}
}
