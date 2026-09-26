using System;
using System.Collections.Generic;

[Serializable]
public class zbShop3CsvTableData
{
	public Dictionary<int, zbShop3CsvRecord> records = new Dictionary<int, zbShop3CsvRecord>();

	public static zbShop3CsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbShop3CsvTableData zbShop3CsvTableData2 = new zbShop3CsvTableData();
		Dictionary<int, zbShop3CsvRecord> dictionary = new Dictionary<int, zbShop3CsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbShop3CsvRecord.Read(zbBinReader2);
		}
		zbShop3CsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbShop3CsvTableData2;
	}
}
