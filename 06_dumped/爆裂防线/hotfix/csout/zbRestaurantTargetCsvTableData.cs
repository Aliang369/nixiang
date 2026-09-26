using System;
using System.Collections.Generic;

[Serializable]
public class zbRestaurantTargetCsvTableData
{
	public Dictionary<int, zbRestaurantTargetCsvRecord> records = new Dictionary<int, zbRestaurantTargetCsvRecord>();

	public static zbRestaurantTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRestaurantTargetCsvTableData zbRestaurantTargetCsvTableData2 = new zbRestaurantTargetCsvTableData();
		Dictionary<int, zbRestaurantTargetCsvRecord> dictionary = new Dictionary<int, zbRestaurantTargetCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbRestaurantTargetCsvRecord.Read(zbBinReader2);
		}
		zbRestaurantTargetCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbRestaurantTargetCsvTableData2;
	}
}
