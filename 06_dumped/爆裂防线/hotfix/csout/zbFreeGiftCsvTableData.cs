using System;
using System.Collections.Generic;

[Serializable]
public class zbFreeGiftCsvTableData
{
	public Dictionary<int, zbFreeGiftCsvRecord> records = new Dictionary<int, zbFreeGiftCsvRecord>();

	public static zbFreeGiftCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFreeGiftCsvTableData zbFreeGiftCsvTableData2 = new zbFreeGiftCsvTableData();
		Dictionary<int, zbFreeGiftCsvRecord> dictionary = new Dictionary<int, zbFreeGiftCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFreeGiftCsvRecord.Read(zbBinReader2);
		}
		zbFreeGiftCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFreeGiftCsvTableData2;
	}
}
