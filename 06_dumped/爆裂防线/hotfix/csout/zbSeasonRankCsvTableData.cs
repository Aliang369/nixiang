using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonRankCsvTableData
{
	public Dictionary<int, zbSeasonRankCsvRecord> records = new Dictionary<int, zbSeasonRankCsvRecord>();

	public static zbSeasonRankCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonRankCsvTableData zbSeasonRankCsvTableData2 = new zbSeasonRankCsvTableData();
		Dictionary<int, zbSeasonRankCsvRecord> dictionary = new Dictionary<int, zbSeasonRankCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonRankCsvRecord.Read(zbBinReader2);
		}
		zbSeasonRankCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonRankCsvTableData2;
	}
}
