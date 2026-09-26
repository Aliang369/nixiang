using System;
using System.Collections.Generic;

[Serializable]
public class zbQualityCsvTableData
{
	public Dictionary<int, zbQualityCsvRecord> records = new Dictionary<int, zbQualityCsvRecord>();

	public static zbQualityCsvTableData ReadBin(byte[] bytes)
	{
		zbBinReader zbBinReader2 = zbBinReader.Create(bytes);
		zbQualityCsvTableData zbQualityCsvTableData2 = new zbQualityCsvTableData();
		Dictionary<int, zbQualityCsvRecord> dictionary = new Dictionary<int, zbQualityCsvRecord>(zbBinReader2.RecordCount);
		for (int i = 0; i < zbBinReader2.RecordCount; i++)
		{
			int key = zbBinReader2.ReadInt();
			dictionary[key] = zbQualityCsvRecord.Read(zbBinReader2);
		}
		zbQualityCsvTableData2.records = dictionary;
		zbBinReader2.ValidateEnd();
		return zbQualityCsvTableData2;
	}
}
