using System;
using System.Collections.Generic;

[Serializable]
public class zbSubscribeShopCsvTableData
{
	public Dictionary<int, zbSubscribeShopCsvRecord> records = new Dictionary<int, zbSubscribeShopCsvRecord>();

	public static zbSubscribeShopCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSubscribeShopCsvTableData zbSubscribeShopCsvTableData2 = new zbSubscribeShopCsvTableData();
		Dictionary<int, zbSubscribeShopCsvRecord> dictionary = new Dictionary<int, zbSubscribeShopCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSubscribeShopCsvRecord.Read(zbBinReader2);
		}
		zbSubscribeShopCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSubscribeShopCsvTableData2;
	}
}
