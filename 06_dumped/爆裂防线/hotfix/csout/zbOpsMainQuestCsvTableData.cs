using System;
using System.Collections.Generic;

[Serializable]
public class zbOpsMainQuestCsvTableData
{
	public Dictionary<int, zbOpsMainQuestCsvRecord> records = new Dictionary<int, zbOpsMainQuestCsvRecord>();

	public static zbOpsMainQuestCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbOpsMainQuestCsvTableData zbOpsMainQuestCsvTableData2 = new zbOpsMainQuestCsvTableData();
		Dictionary<int, zbOpsMainQuestCsvRecord> dictionary = new Dictionary<int, zbOpsMainQuestCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbOpsMainQuestCsvRecord.Read(zbBinReader2);
		}
		zbOpsMainQuestCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbOpsMainQuestCsvTableData2;
	}
}
