using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldLairLevelCsvTableData
{
	public Dictionary<int, zbWorldLairLevelCsvRecord> records = new Dictionary<int, zbWorldLairLevelCsvRecord>();

	public static zbWorldLairLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldLairLevelCsvTableData zbWorldLairLevelCsvTableData2 = new zbWorldLairLevelCsvTableData();
		Dictionary<int, zbWorldLairLevelCsvRecord> dictionary = new Dictionary<int, zbWorldLairLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldLairLevelCsvRecord.Read(zbBinReader2);
		}
		zbWorldLairLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldLairLevelCsvTableData2;
	}
}
