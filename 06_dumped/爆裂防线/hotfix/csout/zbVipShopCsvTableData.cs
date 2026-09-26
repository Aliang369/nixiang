using System;
using System.Collections.Generic;

[Serializable]
public class zbVipShopCsvTableData
{
	public Dictionary<int, zbVipShopCsvRecord> records = new Dictionary<int, zbVipShopCsvRecord>();

	public static zbVipShopCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipShopCsvTableData zbVipShopCsvTableData2 = new zbVipShopCsvTableData();
		Dictionary<int, zbVipShopCsvRecord> dictionary = new Dictionary<int, zbVipShopCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipShopCsvRecord.Read(zbBinReader2);
		}
		zbVipShopCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipShopCsvTableData2;
	}
}
