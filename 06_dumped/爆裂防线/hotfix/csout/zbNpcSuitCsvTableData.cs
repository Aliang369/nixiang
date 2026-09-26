using System;
using System.Collections.Generic;

[Serializable]
public class zbNpcSuitCsvTableData
{
	public Dictionary<int, zbNpcSuitCsvRecord> records = new Dictionary<int, zbNpcSuitCsvRecord>();

	public static zbNpcSuitCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbNpcSuitCsvTableData zbNpcSuitCsvTableData2 = new zbNpcSuitCsvTableData();
		Dictionary<int, zbNpcSuitCsvRecord> dictionary = new Dictionary<int, zbNpcSuitCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbNpcSuitCsvRecord.Read(zbBinReader2);
		}
		zbNpcSuitCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbNpcSuitCsvTableData2;
	}
}
