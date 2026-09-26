using System;
using System.Collections.Generic;

[Serializable]
public class zbVipFreeExpCsvTableData
{
	public Dictionary<int, zbVipFreeExpCsvRecord> records = new Dictionary<int, zbVipFreeExpCsvRecord>();

	public static zbVipFreeExpCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipFreeExpCsvTableData zbVipFreeExpCsvTableData2 = new zbVipFreeExpCsvTableData();
		Dictionary<int, zbVipFreeExpCsvRecord> dictionary = new Dictionary<int, zbVipFreeExpCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipFreeExpCsvRecord.Read(zbBinReader2);
		}
		zbVipFreeExpCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipFreeExpCsvTableData2;
	}
}
