using System;
using System.Collections.Generic;

[Serializable]
public class zbHolidayTaskLevelCsvTableData
{
	public Dictionary<int, zbHolidayTaskLevelCsvRecord> records = new Dictionary<int, zbHolidayTaskLevelCsvRecord>();

	public static zbHolidayTaskLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHolidayTaskLevelCsvTableData zbHolidayTaskLevelCsvTableData2 = new zbHolidayTaskLevelCsvTableData();
		Dictionary<int, zbHolidayTaskLevelCsvRecord> dictionary = new Dictionary<int, zbHolidayTaskLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHolidayTaskLevelCsvRecord.Read(zbBinReader2);
		}
		zbHolidayTaskLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHolidayTaskLevelCsvTableData2;
	}
}
