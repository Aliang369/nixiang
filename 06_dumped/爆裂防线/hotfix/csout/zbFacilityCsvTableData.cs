using System;
using System.Collections.Generic;

[Serializable]
public class zbFacilityCsvTableData
{
	public Dictionary<int, zbFacilityCsvRecord> records = new Dictionary<int, zbFacilityCsvRecord>();

	public static zbFacilityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFacilityCsvTableData zbFacilityCsvTableData2 = new zbFacilityCsvTableData();
		Dictionary<int, zbFacilityCsvRecord> dictionary = new Dictionary<int, zbFacilityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFacilityCsvRecord.Read(zbBinReader2);
		}
		zbFacilityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFacilityCsvTableData2;
	}
}
