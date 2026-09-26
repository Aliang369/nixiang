using System;
using System.Collections.Generic;

[Serializable]
public class zbPeakMatchCsvTableData
{
	public Dictionary<int, zbPeakMatchCsvRecord> records = new Dictionary<int, zbPeakMatchCsvRecord>();

	public static zbPeakMatchCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPeakMatchCsvTableData zbPeakMatchCsvTableData2 = new zbPeakMatchCsvTableData();
		Dictionary<int, zbPeakMatchCsvRecord> dictionary = new Dictionary<int, zbPeakMatchCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPeakMatchCsvRecord.Read(zbBinReader2);
		}
		zbPeakMatchCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPeakMatchCsvTableData2;
	}
}
