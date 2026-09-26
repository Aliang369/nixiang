using System;
using System.Collections.Generic;

[Serializable]
public class zbFacilityMapCsvTableData
{
	public Dictionary<int, zbFacilityMapCsvRecord> records = new Dictionary<int, zbFacilityMapCsvRecord>();

	public static zbFacilityMapCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFacilityMapCsvTableData zbFacilityMapCsvTableData2 = new zbFacilityMapCsvTableData();
		Dictionary<int, zbFacilityMapCsvRecord> dictionary = new Dictionary<int, zbFacilityMapCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFacilityMapCsvRecord.Read(zbBinReader2);
		}
		zbFacilityMapCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFacilityMapCsvTableData2;
	}
}
