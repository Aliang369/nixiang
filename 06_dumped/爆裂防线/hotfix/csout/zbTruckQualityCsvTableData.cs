using System;
using System.Collections.Generic;

[Serializable]
public class zbTruckQualityCsvTableData
{
	public Dictionary<int, zbTruckQualityCsvRecord> records = new Dictionary<int, zbTruckQualityCsvRecord>();

	public static zbTruckQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbTruckQualityCsvTableData zbTruckQualityCsvTableData2 = new zbTruckQualityCsvTableData();
		Dictionary<int, zbTruckQualityCsvRecord> dictionary = new Dictionary<int, zbTruckQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbTruckQualityCsvRecord.Read(zbBinReader2);
		}
		zbTruckQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbTruckQualityCsvTableData2;
	}
}
