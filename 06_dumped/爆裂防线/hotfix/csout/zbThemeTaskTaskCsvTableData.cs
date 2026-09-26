using System;
using System.Collections.Generic;

[Serializable]
public class zbThemeTaskTaskCsvTableData
{
	public Dictionary<int, zbThemeTaskTaskCsvRecord> records = new Dictionary<int, zbThemeTaskTaskCsvRecord>();

	public static zbThemeTaskTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbThemeTaskTaskCsvTableData zbThemeTaskTaskCsvTableData2 = new zbThemeTaskTaskCsvTableData();
		Dictionary<int, zbThemeTaskTaskCsvRecord> dictionary = new Dictionary<int, zbThemeTaskTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbThemeTaskTaskCsvRecord.Read(zbBinReader2);
		}
		zbThemeTaskTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbThemeTaskTaskCsvTableData2;
	}
}
