using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonResourceCsvTableData
{
	public Dictionary<int, zbSeasonResourceCsvRecord> records = new Dictionary<int, zbSeasonResourceCsvRecord>();

	public static zbSeasonResourceCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonResourceCsvTableData zbSeasonResourceCsvTableData2 = new zbSeasonResourceCsvTableData();
		Dictionary<int, zbSeasonResourceCsvRecord> dictionary = new Dictionary<int, zbSeasonResourceCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonResourceCsvRecord.Read(zbBinReader2);
		}
		zbSeasonResourceCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonResourceCsvTableData2;
	}
}
