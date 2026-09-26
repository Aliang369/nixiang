using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerSuitCsvTableData
{
	public Dictionary<int, zbPlayerSuitCsvRecord> records = new Dictionary<int, zbPlayerSuitCsvRecord>();

	public static zbPlayerSuitCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerSuitCsvTableData zbPlayerSuitCsvTableData2 = new zbPlayerSuitCsvTableData();
		Dictionary<int, zbPlayerSuitCsvRecord> dictionary = new Dictionary<int, zbPlayerSuitCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerSuitCsvRecord.Read(zbBinReader2);
		}
		zbPlayerSuitCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerSuitCsvTableData2;
	}
}
