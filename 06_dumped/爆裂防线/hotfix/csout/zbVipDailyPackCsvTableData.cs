using System;
using System.Collections.Generic;

[Serializable]
public class zbVipDailyPackCsvTableData
{
	public Dictionary<int, zbVipDailyPackCsvRecord> records = new Dictionary<int, zbVipDailyPackCsvRecord>();

	public static zbVipDailyPackCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipDailyPackCsvTableData zbVipDailyPackCsvTableData2 = new zbVipDailyPackCsvTableData();
		Dictionary<int, zbVipDailyPackCsvRecord> dictionary = new Dictionary<int, zbVipDailyPackCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipDailyPackCsvRecord.Read(zbBinReader2);
		}
		zbVipDailyPackCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipDailyPackCsvTableData2;
	}
}
