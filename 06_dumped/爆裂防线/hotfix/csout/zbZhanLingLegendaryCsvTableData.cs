using System;
using System.Collections.Generic;

[Serializable]
public class zbZhanLingLegendaryCsvTableData
{
	public Dictionary<int, zbZhanLingLegendaryCsvRecord> records = new Dictionary<int, zbZhanLingLegendaryCsvRecord>();

	public static zbZhanLingLegendaryCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbZhanLingLegendaryCsvTableData zbZhanLingLegendaryCsvTableData2 = new zbZhanLingLegendaryCsvTableData();
		Dictionary<int, zbZhanLingLegendaryCsvRecord> dictionary = new Dictionary<int, zbZhanLingLegendaryCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbZhanLingLegendaryCsvRecord.Read(zbBinReader2);
		}
		zbZhanLingLegendaryCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbZhanLingLegendaryCsvTableData2;
	}
}
