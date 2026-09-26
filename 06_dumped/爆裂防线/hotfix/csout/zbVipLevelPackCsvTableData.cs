using System;
using System.Collections.Generic;

[Serializable]
public class zbVipLevelPackCsvTableData
{
	public Dictionary<int, zbVipLevelPackCsvRecord> records = new Dictionary<int, zbVipLevelPackCsvRecord>();

	public static zbVipLevelPackCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipLevelPackCsvTableData zbVipLevelPackCsvTableData2 = new zbVipLevelPackCsvTableData();
		Dictionary<int, zbVipLevelPackCsvRecord> dictionary = new Dictionary<int, zbVipLevelPackCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipLevelPackCsvRecord.Read(zbBinReader2);
		}
		zbVipLevelPackCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipLevelPackCsvTableData2;
	}
}
