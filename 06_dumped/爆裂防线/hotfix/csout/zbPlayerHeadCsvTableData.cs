using System;
using System.Collections.Generic;

[Serializable]
public class zbPlayerHeadCsvTableData
{
	public Dictionary<int, zbPlayerHeadCsvRecord> records = new Dictionary<int, zbPlayerHeadCsvRecord>();

	public static zbPlayerHeadCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPlayerHeadCsvTableData zbPlayerHeadCsvTableData2 = new zbPlayerHeadCsvTableData();
		Dictionary<int, zbPlayerHeadCsvRecord> dictionary = new Dictionary<int, zbPlayerHeadCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPlayerHeadCsvRecord.Read(zbBinReader2);
		}
		zbPlayerHeadCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPlayerHeadCsvTableData2;
	}
}
