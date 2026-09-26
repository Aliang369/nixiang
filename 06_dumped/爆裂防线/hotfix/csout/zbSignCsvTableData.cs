using System;
using System.Collections.Generic;

[Serializable]
public class zbSignCsvTableData
{
	public Dictionary<int, zbSignCsvRecord> records = new Dictionary<int, zbSignCsvRecord>();

	public static zbSignCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbSignCsvTableData zbSignCsvTableData2 = new zbSignCsvTableData();
		Dictionary<int, zbSignCsvRecord> dictionary = new Dictionary<int, zbSignCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbSignCsvRecord.Read(zbBinReader2);
		}
		zbSignCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbSignCsvTableData2;
	}
}
