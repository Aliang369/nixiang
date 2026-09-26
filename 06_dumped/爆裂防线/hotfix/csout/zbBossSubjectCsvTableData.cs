using System;
using System.Collections.Generic;

[Serializable]
public class zbBossSubjectCsvTableData
{
	public Dictionary<int, zbBossSubjectCsvRecord> records = new Dictionary<int, zbBossSubjectCsvRecord>();

	public static zbBossSubjectCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossSubjectCsvTableData zbBossSubjectCsvTableData2 = new zbBossSubjectCsvTableData();
		Dictionary<int, zbBossSubjectCsvRecord> dictionary = new Dictionary<int, zbBossSubjectCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBossSubjectCsvRecord.Read(zbBinReader2);
		}
		zbBossSubjectCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBossSubjectCsvTableData2;
	}
}
