using System;
using System.Collections.Generic;

[Serializable]
public class zbBossTaskCsvTableData
{
	public Dictionary<int, zbBossTaskCsvRecord> records = new Dictionary<int, zbBossTaskCsvRecord>();

	public static zbBossTaskCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossTaskCsvTableData zbBossTaskCsvTableData2 = new zbBossTaskCsvTableData();
		Dictionary<int, zbBossTaskCsvRecord> dictionary = new Dictionary<int, zbBossTaskCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBossTaskCsvRecord.Read(zbBinReader2);
		}
		zbBossTaskCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBossTaskCsvTableData2;
	}
}
