using System;
using System.Collections.Generic;

[Serializable]
public class zbZhanLingKillTargetCsvTableData
{
	public Dictionary<int, zbZhanLingKillTargetCsvRecord> records = new Dictionary<int, zbZhanLingKillTargetCsvRecord>();

	public static zbZhanLingKillTargetCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbZhanLingKillTargetCsvTableData zbZhanLingKillTargetCsvTableData2 = new zbZhanLingKillTargetCsvTableData();
		Dictionary<int, zbZhanLingKillTargetCsvRecord> dictionary = new Dictionary<int, zbZhanLingKillTargetCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbZhanLingKillTargetCsvRecord.Read(zbBinReader2);
		}
		zbZhanLingKillTargetCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbZhanLingKillTargetCsvTableData2;
	}
}
