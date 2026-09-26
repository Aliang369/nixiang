using System;
using System.Collections.Generic;

[Serializable]
public class zbBossLevelCsvTableData
{
	public Dictionary<int, zbBossLevelCsvRecord> records = new Dictionary<int, zbBossLevelCsvRecord>();

	public static zbBossLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossLevelCsvTableData zbBossLevelCsvTableData2 = new zbBossLevelCsvTableData();
		Dictionary<int, zbBossLevelCsvRecord> dictionary = new Dictionary<int, zbBossLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBossLevelCsvRecord.Read(zbBinReader2);
		}
		zbBossLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBossLevelCsvTableData2;
	}
}
