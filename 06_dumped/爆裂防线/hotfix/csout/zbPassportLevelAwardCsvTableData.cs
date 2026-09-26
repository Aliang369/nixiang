using System;
using System.Collections.Generic;

[Serializable]
public class zbPassportLevelAwardCsvTableData
{
	public Dictionary<int, zbPassportLevelAwardCsvRecord> records = new Dictionary<int, zbPassportLevelAwardCsvRecord>();

	public static zbPassportLevelAwardCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbPassportLevelAwardCsvTableData zbPassportLevelAwardCsvTableData2 = new zbPassportLevelAwardCsvTableData();
		Dictionary<int, zbPassportLevelAwardCsvRecord> dictionary = new Dictionary<int, zbPassportLevelAwardCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbPassportLevelAwardCsvRecord.Read(zbBinReader2);
		}
		zbPassportLevelAwardCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbPassportLevelAwardCsvTableData2;
	}
}
