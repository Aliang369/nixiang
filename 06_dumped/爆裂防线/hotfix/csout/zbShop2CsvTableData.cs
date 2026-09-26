using System;
using System.Collections.Generic;

[Serializable]
public class zbShop2CsvTableData
{
	public Dictionary<int, zbShop2CsvRecord> records = new Dictionary<int, zbShop2CsvRecord>();

	public static zbShop2CsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbShop2CsvTableData zbShop2CsvTableData2 = new zbShop2CsvTableData();
		Dictionary<int, zbShop2CsvRecord> dictionary = new Dictionary<int, zbShop2CsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbShop2CsvRecord.Read(zbBinReader2);
		}
		zbShop2CsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbShop2CsvTableData2;
	}
}
