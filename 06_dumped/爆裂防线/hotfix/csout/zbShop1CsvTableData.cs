using System;
using System.Collections.Generic;

[Serializable]
public class zbShop1CsvTableData
{
	public Dictionary<int, zbShop1CsvRecord> records = new Dictionary<int, zbShop1CsvRecord>();

	public static zbShop1CsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbShop1CsvTableData zbShop1CsvTableData2 = new zbShop1CsvTableData();
		Dictionary<int, zbShop1CsvRecord> dictionary = new Dictionary<int, zbShop1CsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbShop1CsvRecord.Read(zbBinReader2);
		}
		zbShop1CsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbShop1CsvTableData2;
	}
}
