using System;
using System.Collections.Generic;

[Serializable]
public class zbBossTaskTargetCsvTableData
{
	public Dictionary<int, zbBossTaskTargetCsvRecord> records = new Dictionary<int, zbBossTaskTargetCsvRecord>();

	public static zbBossTaskTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbBossTaskTargetCsvTableData zbBossTaskTargetCsvTableData2 = new zbBossTaskTargetCsvTableData();
		Dictionary<int, zbBossTaskTargetCsvRecord> dictionary = new Dictionary<int, zbBossTaskTargetCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbBossTaskTargetCsvRecord.Read(zbBinReader2);
		}
		zbBossTaskTargetCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbBossTaskTargetCsvTableData2;
	}
}
