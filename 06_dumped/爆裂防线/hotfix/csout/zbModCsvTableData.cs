using System;
using System.Collections.Generic;

[Serializable]
public class zbModCsvTableData
{
	public Dictionary<int, zbModCsvRecord> records = new Dictionary<int, zbModCsvRecord>();

	public static zbModCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbModCsvTableData zbModCsvTableData2 = new zbModCsvTableData();
		Dictionary<int, zbModCsvRecord> dictionary = new Dictionary<int, zbModCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbModCsvRecord.Read(zbBinReader2);
		}
		zbModCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbModCsvTableData2;
	}
}
