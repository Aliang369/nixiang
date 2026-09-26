using System;
using System.Collections.Generic;

[Serializable]
public class zbBoxCsvTableData
{
	public Dictionary<int, zbBoxCsvRecord> records = new Dictionary<int, zbBoxCsvRecord>();

	public static zbBoxCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBoxCsvTableData zbBoxCsvTableData2 = new zbBoxCsvTableData();
		Dictionary<int, zbBoxCsvRecord> dictionary = new Dictionary<int, zbBoxCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBoxCsvRecord.Read(zbBinReader2);
		}
		zbBoxCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBoxCsvTableData2;
	}
}
