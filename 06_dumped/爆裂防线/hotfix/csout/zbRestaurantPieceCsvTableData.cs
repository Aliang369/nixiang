using System;
using System.Collections.Generic;

[Serializable]
public class zbRestaurantPieceCsvTableData
{
	public Dictionary<int, zbRestaurantPieceCsvRecord> records = new Dictionary<int, zbRestaurantPieceCsvRecord>();

	public static zbRestaurantPieceCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbRestaurantPieceCsvTableData zbRestaurantPieceCsvTableData2 = new zbRestaurantPieceCsvTableData();
		Dictionary<int, zbRestaurantPieceCsvRecord> dictionary = new Dictionary<int, zbRestaurantPieceCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbRestaurantPieceCsvRecord.Read(zbBinReader2);
		}
		zbRestaurantPieceCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbRestaurantPieceCsvTableData2;
	}
}
