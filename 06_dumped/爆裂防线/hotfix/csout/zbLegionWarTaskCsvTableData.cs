using System;
using System.Collections.Generic;

[Serializable]
public class zbLegionWarTaskCsvTableData
{
	public Dictionary<int, zbLegionWarTaskCsvRecord> records = new Dictionary<int, zbLegionWarTaskCsvRecord>();

	public static zbLegionWarTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbLegionWarTaskCsvTableData zbLegionWarTaskCsvTableData2 = new zbLegionWarTaskCsvTableData();
		Dictionary<int, zbLegionWarTaskCsvRecord> dictionary = new Dictionary<int, zbLegionWarTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbLegionWarTaskCsvRecord.Read(zbBinReader2);
		}
		zbLegionWarTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbLegionWarTaskCsvTableData2;
	}
}
