using System;
using System.Collections.Generic;

[Serializable]
public class zbFacilityRandPoolCsvTableData
{
	public Dictionary<int, zbFacilityRandPoolCsvRecord> records = new Dictionary<int, zbFacilityRandPoolCsvRecord>();

	public static zbFacilityRandPoolCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFacilityRandPoolCsvTableData zbFacilityRandPoolCsvTableData2 = new zbFacilityRandPoolCsvTableData();
		Dictionary<int, zbFacilityRandPoolCsvRecord> dictionary = new Dictionary<int, zbFacilityRandPoolCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFacilityRandPoolCsvRecord.Read(zbBinReader2);
		}
		zbFacilityRandPoolCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFacilityRandPoolCsvTableData2;
	}
}
