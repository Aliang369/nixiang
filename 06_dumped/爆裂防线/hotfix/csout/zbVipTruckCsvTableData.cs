using System;
using System.Collections.Generic;

[Serializable]
public class zbVipTruckCsvTableData
{
	public Dictionary<int, zbVipTruckCsvRecord> records = new Dictionary<int, zbVipTruckCsvRecord>();

	public static zbVipTruckCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipTruckCsvTableData zbVipTruckCsvTableData2 = new zbVipTruckCsvTableData();
		Dictionary<int, zbVipTruckCsvRecord> dictionary = new Dictionary<int, zbVipTruckCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipTruckCsvRecord.Read(zbBinReader2);
		}
		zbVipTruckCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipTruckCsvTableData2;
	}
}
