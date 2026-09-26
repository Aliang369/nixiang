using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldLairCsvTableData
{
	public Dictionary<int, zbWorldLairCsvRecord> records = new Dictionary<int, zbWorldLairCsvRecord>();

	public static zbWorldLairCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldLairCsvTableData zbWorldLairCsvTableData2 = new zbWorldLairCsvTableData();
		Dictionary<int, zbWorldLairCsvRecord> dictionary = new Dictionary<int, zbWorldLairCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldLairCsvRecord.Read(zbBinReader2);
		}
		zbWorldLairCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldLairCsvTableData2;
	}
}
