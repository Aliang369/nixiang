using System;
using System.Collections.Generic;

[Serializable]
public class zbEndlessHordeLevelCsvTableData
{
	public Dictionary<int, zbEndlessHordeLevelCsvRecord> records = new Dictionary<int, zbEndlessHordeLevelCsvRecord>();

	public static zbEndlessHordeLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbEndlessHordeLevelCsvTableData zbEndlessHordeLevelCsvTableData2 = new zbEndlessHordeLevelCsvTableData();
		Dictionary<int, zbEndlessHordeLevelCsvRecord> dictionary = new Dictionary<int, zbEndlessHordeLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbEndlessHordeLevelCsvRecord.Read(zbBinReader2);
		}
		zbEndlessHordeLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbEndlessHordeLevelCsvTableData2;
	}
}
