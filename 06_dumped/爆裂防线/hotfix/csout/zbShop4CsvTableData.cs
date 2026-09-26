using System;
using System.Collections.Generic;

[Serializable]
public class zbShop4CsvTableData
{
	public Dictionary<int, zbShop4CsvRecord> records = new Dictionary<int, zbShop4CsvRecord>();

	public static zbShop4CsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbShop4CsvTableData zbShop4CsvTableData2 = new zbShop4CsvTableData();
		Dictionary<int, zbShop4CsvRecord> dictionary = new Dictionary<int, zbShop4CsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbShop4CsvRecord.Read(zbBinReader2);
		}
		zbShop4CsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbShop4CsvTableData2;
	}
}
