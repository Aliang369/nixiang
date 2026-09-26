using System;
using System.Collections.Generic;

[Serializable]
public class zbZhanlingElementCsvTableData
{
	public Dictionary<int, zbZhanlingElementCsvRecord> records = new Dictionary<int, zbZhanlingElementCsvRecord>();

	public static zbZhanlingElementCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbZhanlingElementCsvTableData zbZhanlingElementCsvTableData2 = new zbZhanlingElementCsvTableData();
		Dictionary<int, zbZhanlingElementCsvRecord> dictionary = new Dictionary<int, zbZhanlingElementCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbZhanlingElementCsvRecord.Read(zbBinReader2);
		}
		zbZhanlingElementCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbZhanlingElementCsvTableData2;
	}
}
