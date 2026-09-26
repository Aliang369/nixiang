using System;
using System.Collections.Generic;

[Serializable]
public class zbFacilityShopCsvTableData
{
	public Dictionary<int, zbFacilityShopCsvRecord> records = new Dictionary<int, zbFacilityShopCsvRecord>();

	public static zbFacilityShopCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFacilityShopCsvTableData zbFacilityShopCsvTableData2 = new zbFacilityShopCsvTableData();
		Dictionary<int, zbFacilityShopCsvRecord> dictionary = new Dictionary<int, zbFacilityShopCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFacilityShopCsvRecord.Read(zbBinReader2);
		}
		zbFacilityShopCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFacilityShopCsvTableData2;
	}
}
