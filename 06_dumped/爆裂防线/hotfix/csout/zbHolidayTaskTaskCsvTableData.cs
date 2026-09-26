using System;
using System.Collections.Generic;

[Serializable]
public class zbHolidayTaskTaskCsvTableData
{
	public Dictionary<int, zbHolidayTaskTaskCsvRecord> records = new Dictionary<int, zbHolidayTaskTaskCsvRecord>();

	public static zbHolidayTaskTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHolidayTaskTaskCsvTableData zbHolidayTaskTaskCsvTableData2 = new zbHolidayTaskTaskCsvTableData();
		Dictionary<int, zbHolidayTaskTaskCsvRecord> dictionary = new Dictionary<int, zbHolidayTaskTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHolidayTaskTaskCsvRecord.Read(zbBinReader2);
		}
		zbHolidayTaskTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHolidayTaskTaskCsvTableData2;
	}
}
