using System;
using System.Collections.Generic;

[Serializable]
public class zbSeasonAreaCsvTableData
{
	public Dictionary<int, zbSeasonAreaCsvRecord> records = new Dictionary<int, zbSeasonAreaCsvRecord>();

	public static zbSeasonAreaCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSeasonAreaCsvTableData zbSeasonAreaCsvTableData2 = new zbSeasonAreaCsvTableData();
		Dictionary<int, zbSeasonAreaCsvRecord> dictionary = new Dictionary<int, zbSeasonAreaCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSeasonAreaCsvRecord.Read(zbBinReader2);
		}
		zbSeasonAreaCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSeasonAreaCsvTableData2;
	}
}
