using System;
using System.Collections.Generic;

[Serializable]
public class zbWorldEventBoxCsvTableData
{
	public Dictionary<int, zbWorldEventBoxCsvRecord> records = new Dictionary<int, zbWorldEventBoxCsvRecord>();

	public static zbWorldEventBoxCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbWorldEventBoxCsvTableData zbWorldEventBoxCsvTableData2 = new zbWorldEventBoxCsvTableData();
		Dictionary<int, zbWorldEventBoxCsvRecord> dictionary = new Dictionary<int, zbWorldEventBoxCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbWorldEventBoxCsvRecord.Read(zbBinReader2);
		}
		zbWorldEventBoxCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbWorldEventBoxCsvTableData2;
	}
}
