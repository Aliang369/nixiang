using System;
using System.Collections.Generic;

[Serializable]
public class zbGemCsvTableData
{
	public Dictionary<int, zbGemCsvRecord> records = new Dictionary<int, zbGemCsvRecord>();

	public static zbGemCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbGemCsvTableData zbGemCsvTableData2 = new zbGemCsvTableData();
		Dictionary<int, zbGemCsvRecord> dictionary = new Dictionary<int, zbGemCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbGemCsvRecord.Read(zbBinReader2);
		}
		zbGemCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbGemCsvTableData2;
	}
}
