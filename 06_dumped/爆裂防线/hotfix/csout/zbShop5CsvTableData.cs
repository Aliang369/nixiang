using System;
using System.Collections.Generic;

[Serializable]
public class zbShop5CsvTableData
{
	public Dictionary<int, zbShop5CsvRecord> records = new Dictionary<int, zbShop5CsvRecord>();

	public static zbShop5CsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbShop5CsvTableData zbShop5CsvTableData2 = new zbShop5CsvTableData();
		Dictionary<int, zbShop5CsvRecord> dictionary = new Dictionary<int, zbShop5CsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbShop5CsvRecord.Read(zbBinReader2);
		}
		zbShop5CsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbShop5CsvTableData2;
	}
}
