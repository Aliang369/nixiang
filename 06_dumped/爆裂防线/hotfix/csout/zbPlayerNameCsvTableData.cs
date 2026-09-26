using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerNameCsvTableData
{
	public Dictionary<int, zbPlayerNameCsvRecord> records = new Dictionary<int, zbPlayerNameCsvRecord>();

	public static zbPlayerNameCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerNameCsvTableData zbPlayerNameCsvTableData2 = new zbPlayerNameCsvTableData();
		Dictionary<int, zbPlayerNameCsvRecord> dictionary = new Dictionary<int, zbPlayerNameCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerNameCsvRecord.Read(zbBinReader2);
		}
		zbPlayerNameCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerNameCsvTableData2;
	}
}
