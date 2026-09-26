using System;
using System.Collections.Generic;

[Serializable]
public class zbAdvertisementCsvTableData
{
	public Dictionary<int, zbAdvertisementCsvRecord> records = new Dictionary<int, zbAdvertisementCsvRecord>();

	public static zbAdvertisementCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbAdvertisementCsvTableData zbAdvertisementCsvTableData2 = new zbAdvertisementCsvTableData();
		Dictionary<int, zbAdvertisementCsvRecord> dictionary = new Dictionary<int, zbAdvertisementCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbAdvertisementCsvRecord.Read(zbBinReader2);
		}
		zbAdvertisementCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbAdvertisementCsvTableData2;
	}
}
