using System;
using System.Collections.Generic;

[Serializable]
public class zbElementTrialLevelCsvTableData
{
	public Dictionary<int, zbElementTrialLevelCsvRecord> records = new Dictionary<int, zbElementTrialLevelCsvRecord>();

	public static zbElementTrialLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbElementTrialLevelCsvTableData zbElementTrialLevelCsvTableData2 = new zbElementTrialLevelCsvTableData();
		Dictionary<int, zbElementTrialLevelCsvRecord> dictionary = new Dictionary<int, zbElementTrialLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbElementTrialLevelCsvRecord.Read(zbBinReader2);
		}
		zbElementTrialLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbElementTrialLevelCsvTableData2;
	}
}
