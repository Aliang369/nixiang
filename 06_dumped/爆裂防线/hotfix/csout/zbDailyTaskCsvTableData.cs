using System;
using System.Collections.Generic;

[Serializable]
public class zbDailyTaskCsvTableData
{
	public Dictionary<int, zbDailyTaskCsvRecord> records = new Dictionary<int, zbDailyTaskCsvRecord>();

	public static zbDailyTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbDailyTaskCsvTableData zbDailyTaskCsvTableData2 = new zbDailyTaskCsvTableData();
		Dictionary<int, zbDailyTaskCsvRecord> dictionary = new Dictionary<int, zbDailyTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbDailyTaskCsvRecord.Read(zbBinReader2);
		}
		zbDailyTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbDailyTaskCsvTableData2;
	}
}
