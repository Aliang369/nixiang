using System;
using System.Collections.Generic;

[Serializable]
public class zbElementTrialCsvTableData
{
	public Dictionary<int, zbElementTrialCsvRecord> records = new Dictionary<int, zbElementTrialCsvRecord>();

	public static zbElementTrialCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbElementTrialCsvTableData zbElementTrialCsvTableData2 = new zbElementTrialCsvTableData();
		Dictionary<int, zbElementTrialCsvRecord> dictionary = new Dictionary<int, zbElementTrialCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbElementTrialCsvRecord.Read(zbBinReader2);
		}
		zbElementTrialCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbElementTrialCsvTableData2;
	}
}
