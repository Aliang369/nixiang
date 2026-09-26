using System;
using System.Collections.Generic;

[Serializable]
public class zbFusionLevelCsvTableData
{
	public Dictionary<int, zbFusionLevelCsvRecord> records = new Dictionary<int, zbFusionLevelCsvRecord>();

	public static zbFusionLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbFusionLevelCsvTableData zbFusionLevelCsvTableData2 = new zbFusionLevelCsvTableData();
		Dictionary<int, zbFusionLevelCsvRecord> dictionary = new Dictionary<int, zbFusionLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbFusionLevelCsvRecord.Read(zbBinReader2);
		}
		zbFusionLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbFusionLevelCsvTableData2;
	}
}
