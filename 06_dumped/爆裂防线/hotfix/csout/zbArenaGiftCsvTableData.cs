using System;
using System.Collections.Generic;

[Serializable]
public class zbArenaGiftCsvTableData
{
	public Dictionary<int, zbArenaGiftCsvRecord> records = new Dictionary<int, zbArenaGiftCsvRecord>();

	public static zbArenaGiftCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbArenaGiftCsvTableData zbArenaGiftCsvTableData2 = new zbArenaGiftCsvTableData();
		Dictionary<int, zbArenaGiftCsvRecord> dictionary = new Dictionary<int, zbArenaGiftCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbArenaGiftCsvRecord.Read(zbBinReader2);
		}
		zbArenaGiftCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbArenaGiftCsvTableData2;
	}
}
