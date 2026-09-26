using System;
using System.Collections.Generic;

[Serializable]
public class zbPrayLanternAwardCsvTableData
{
	public Dictionary<int, zbPrayLanternAwardCsvRecord> records = new Dictionary<int, zbPrayLanternAwardCsvRecord>();

	public static zbPrayLanternAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPrayLanternAwardCsvTableData zbPrayLanternAwardCsvTableData2 = new zbPrayLanternAwardCsvTableData();
		Dictionary<int, zbPrayLanternAwardCsvRecord> dictionary = new Dictionary<int, zbPrayLanternAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPrayLanternAwardCsvRecord.Read(zbBinReader2);
		}
		zbPrayLanternAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPrayLanternAwardCsvTableData2;
	}
}
