using System;
using System.Collections.Generic;

[Serializable]
public class zbDirectShopCsvTableData
{
	public Dictionary<int, zbDirectShopCsvRecord> records = new Dictionary<int, zbDirectShopCsvRecord>();

	public static zbDirectShopCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDirectShopCsvTableData zbDirectShopCsvTableData2 = new zbDirectShopCsvTableData();
		Dictionary<int, zbDirectShopCsvRecord> dictionary = new Dictionary<int, zbDirectShopCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDirectShopCsvRecord.Read(zbBinReader2);
		}
		zbDirectShopCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDirectShopCsvTableData2;
	}
}
