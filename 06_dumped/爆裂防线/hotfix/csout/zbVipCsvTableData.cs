using System;
using System.Collections.Generic;

[Serializable]
public class zbVipCsvTableData
{
	public Dictionary<int, zbVipCsvRecord> records = new Dictionary<int, zbVipCsvRecord>();

	public static zbVipCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbVipCsvTableData zbVipCsvTableData2 = new zbVipCsvTableData();
		Dictionary<int, zbVipCsvRecord> dictionary = new Dictionary<int, zbVipCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbVipCsvRecord.Read(zbBinReader2);
		}
		zbVipCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbVipCsvTableData2;
	}
}
