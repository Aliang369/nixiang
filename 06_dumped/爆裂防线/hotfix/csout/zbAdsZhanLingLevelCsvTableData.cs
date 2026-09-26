using System;
using System.Collections.Generic;

[Serializable]
public class zbAdsZhanLingLevelCsvTableData
{
	public Dictionary<int, zbAdsZhanLingLevelCsvRecord> records = new Dictionary<int, zbAdsZhanLingLevelCsvRecord>();

	public static zbAdsZhanLingLevelCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbAdsZhanLingLevelCsvTableData zbAdsZhanLingLevelCsvTableData2 = new zbAdsZhanLingLevelCsvTableData();
		Dictionary<int, zbAdsZhanLingLevelCsvRecord> dictionary = new Dictionary<int, zbAdsZhanLingLevelCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbAdsZhanLingLevelCsvRecord.Read(zbBinReader2);
		}
		zbAdsZhanLingLevelCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbAdsZhanLingLevelCsvTableData2;
	}
}
