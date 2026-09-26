using System;
using System.Collections.Generic;

[Serializable]
public class zbBossZhanlingCsvTableData
{
	public Dictionary<int, zbBossZhanlingCsvRecord> records = new Dictionary<int, zbBossZhanlingCsvRecord>();

	public static zbBossZhanlingCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossZhanlingCsvTableData zbBossZhanlingCsvTableData2 = new zbBossZhanlingCsvTableData();
		Dictionary<int, zbBossZhanlingCsvRecord> dictionary = new Dictionary<int, zbBossZhanlingCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBossZhanlingCsvRecord.Read(zbBinReader2);
		}
		zbBossZhanlingCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBossZhanlingCsvTableData2;
	}
}
