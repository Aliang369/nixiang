using System;
using System.Collections.Generic;

[Serializable]
public class zbHeroTrialCsvTableData
{
	public Dictionary<int, zbHeroTrialCsvRecord> records = new Dictionary<int, zbHeroTrialCsvRecord>();

	public static zbHeroTrialCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbHeroTrialCsvTableData zbHeroTrialCsvTableData2 = new zbHeroTrialCsvTableData();
		Dictionary<int, zbHeroTrialCsvRecord> dictionary = new Dictionary<int, zbHeroTrialCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbHeroTrialCsvRecord.Read(zbBinReader2);
		}
		zbHeroTrialCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbHeroTrialCsvTableData2;
	}
}
