using System;
using System.Collections.Generic;

[Serializable]
public class zbMoneyCsvTableData
{
	public Dictionary<int, zbMoneyCsvRecord> records = new Dictionary<int, zbMoneyCsvRecord>();

	public static zbMoneyCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbMoneyCsvTableData zbMoneyCsvTableData2 = new zbMoneyCsvTableData();
		Dictionary<int, zbMoneyCsvRecord> dictionary = new Dictionary<int, zbMoneyCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbMoneyCsvRecord.Read(zbBinReader2);
		}
		zbMoneyCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbMoneyCsvTableData2;
	}
}
